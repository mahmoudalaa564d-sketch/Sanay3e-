
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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

            // JWT configuration
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is missing from configuration.");

            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer is missing from configuration.");

            var jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience is missing from configuration.");

            // Database configuration
            var connectionString =
                builder.Configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Database connection string is missing.");

            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(connectionString));

            // Application services
            builder.Services.AddScoped<PasswordService>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<OtpService>();

            // Structured error responses
            builder.Services.AddProblemDetails();

            // JWT authentication
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
                });

            // Authorization
            builder.Services.AddAuthorization();

            // Controllers and API documentation
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Handle unhandled exceptions without exposing internals.
            app.UseExceptionHandler();

            // OpenAPI and Scalar are available only in Development.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
