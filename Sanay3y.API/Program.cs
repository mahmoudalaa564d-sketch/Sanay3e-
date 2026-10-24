
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Sanay3y.API.Data;
using Sanay3y.API.Services;

namespace Sanay3y.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. JWT configuration
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is missing from configuration.");

            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer is missing from configuration.");

            var jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience is missing from configuration.");

            // 2. Database configuration
            var connectionString =
                builder.Configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Database connection string is missing.");

            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(connectionString));

            // 3. Application services
            builder.Services.AddScoped<PasswordService>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<OtpService>();

            // 4. Structured error responses
            builder.Services.AddProblemDetails();

            // 5. Health checks
            builder.Services.AddHealthChecks();

            // 6. Rate limiting configuration
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (
                    context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode =
                        StatusCodes.Status429TooManyRequests;

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new ProblemDetails
                        {
                            Status =
                                StatusCodes.Status429TooManyRequests,
                            Title = "Too many requests",
                            Detail =
                                "Please wait before trying again."
                        },
                        cancellationToken: cancellationToken);
                };

                // Login: 5 requests per minute per IP
                options.AddPolicy("LoginPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));

                // Registration: 5 requests per 10 minutes per IP
                options.AddPolicy("RegisterPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));

                // Request OTP: 3 requests per 5 minutes per IP
                options.AddPolicy("OtpRequestPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromMinutes(5),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));

                // Verify OTP: 5 requests per 5 minutes per IP
                options.AddPolicy("OtpVerifyPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(5),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));
            });

            // 7. JWT authentication
            builder.Services
                .AddAuthentication(
                    JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwtIssuer,

                            ValidateAudience = true,
                            ValidAudience = jwtAudience,

                            ValidateLifetime = true,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtKey)),

                            ClockSkew = TimeSpan.FromSeconds(30)
                        };

                    // Check current account status whenever a JWT
                    // is successfully validated.
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var principal = context.Principal;

                            var userIdValue =
                                principal?.FindFirstValue(
                                    ClaimTypes.NameIdentifier)
                                ?? principal?.FindFirstValue("sub");

                            if (!int.TryParse(
                                    userIdValue,
                                    out var userId))
                            {
                                context.Fail(
                                    "Invalid token identity.");
                                return;
                            }

                            var dbContext =
                                context.HttpContext.RequestServices
                                    .GetRequiredService<
                                        ApplicationDbContext>();

                            Sanay3y.API.Models.User? user;

                            try
                            {
                                user = await dbContext.Users
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(
                                        u => u.Id == userId,
                                        context.HttpContext
                                            .RequestAborted);
                            }
                            catch (Exception ex)
                                when (ex is not OperationCanceledException)
                            {
                                var logger =
                                    context.HttpContext
                                        .RequestServices
                                        .GetRequiredService<
                                            ILogger<Program>>();

                                logger.LogError(
                                    ex,
                                    "Failed to validate account status for JWT.");

                                // Fail closed if account status
                                // cannot be checked.
                                context.Fail(
                                    "Account status could not be verified.");
                                return;
                            }

                            if (user is null ||
                                user.AccountStatus != "Active")
                            {
                                context.Fail("Account is inactive.");
                                return;
                            }

                            // Refresh role from the database rather
                            // than trusting an old JWT role.
                            var identity =
                                principal?.Identity as ClaimsIdentity;

                            if (identity is null)
                            {
                                context.Fail(
                                    "Invalid token identity.");
                                return;
                            }

                            var oldRoleClaims = identity.Claims
                                .Where(c =>
                                    c.Type == ClaimTypes.Role ||
                                    c.Type == "role" ||
                                    c.Type == "roles")
                                .ToList();

                            foreach (var claim in oldRoleClaims)
                            {
                                identity.RemoveClaim(claim);
                            }

                            identity.AddClaim(
                                new Claim(
                                    ClaimTypes.Role,
                                    user.Role));
                        }
                    };
                });

            // 8. Authorization
            builder.Services.AddAuthorization();

            // 9. Controllers and API documentation
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // 10. Handle unexpected errors
            app.UseExceptionHandler();

            // 11. Security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] =
                    "nosniff";

                context.Response.Headers["X-Frame-Options"] =
                    "DENY";

                context.Response.Headers["Referrer-Policy"] =
                    "no-referrer";

                context.Response.Headers["Permissions-Policy"] =
                    "camera=(), microphone=(), geolocation=()";

                await next();
            });

            // 12. API documentation in Development only
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            // 13. Middleware pipeline
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            // 14. Application endpoints
            app.MapControllers();

            // Liveness: confirms that the application responds.
            app.MapHealthChecks("/health");

            // Readiness: checks whether SQL Server is reachable.
            // Do not expose connection details in the response.
            app.MapGet(
                "/health/ready",
                async Task<IResult> (
                    ApplicationDbContext dbContext,
                    ILogger<Program> logger,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var canConnect =
                            await dbContext.Database.CanConnectAsync(
                                cancellationToken);

                        if (!canConnect)
                        {
                            return Results.StatusCode(
                                StatusCodes.Status503ServiceUnavailable);
                        }

                        return Results.Ok(new { status = "Ready" });
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Database readiness check failed.");

                        return Results.StatusCode(
                            StatusCodes.Status503ServiceUnavailable);
                    }
                });

            app.Run();
        }
    }
}
