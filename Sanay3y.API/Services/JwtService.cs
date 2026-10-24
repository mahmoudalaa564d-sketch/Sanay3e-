
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sanay3y.API.Models;

namespace Sanay3y.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is missing.");

            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer is missing.");

            string audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience is missing.");

            if (Encoding.UTF8.GetByteCount(key) < 32)
            {
                throw new InvalidOperationException(
                    "JWT Key must be at least 32 bytes long for HS256.");
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    string.IsNullOrWhiteSpace(user.FullName)
                        ? user.Id.ToString()
                        : user.FullName),

                new Claim(
                    ClaimTypes.Role,
                    string.IsNullOrWhiteSpace(user.Role)
                        ? "Customer"
                        : user.Role)
            };

            // Add email when available.
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(
                    new Claim(ClaimTypes.Email, user.Email));
            }

            // Add phone number when available.
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.Add(
                    new Claim("phoneNumber", user.PhoneNumber));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
