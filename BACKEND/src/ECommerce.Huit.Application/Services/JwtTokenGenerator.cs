using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Domain.Entities;

namespace ECommerce.Huit.Application.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        public string GenerateToken(User user)
        {
            var keyStr = ConfigurationManager.AppSettings["Jwt:Key"] ?? "super_secret_key_placeholder_32_chars";
            var issuer = ConfigurationManager.AppSettings["Jwt:Issuer"] ?? "ecommerce-huit";
            var audience = ConfigurationManager.AppSettings["Jwt:Audience"] ?? "ecommerce-huit-client";
            var durationMinutes = 1440;
            int.TryParse(ConfigurationManager.AppSettings["Jwt:DurationInMinutes"], out durationMinutes);

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("jti", Guid.NewGuid().ToString())
            };

            var expires = DateTime.UtcNow.AddMinutes(durationMinutes);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
