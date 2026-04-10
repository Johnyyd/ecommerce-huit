using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using HuitShopDB.Services.Interfaces;

namespace HuitShopDB.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        public string GenerateToken(string userId, string email, string role)
        {
            var keyStr = ConfigurationManager.AppSettings["Jwt:Key"] ?? "super_secret_key_placeholder_32_chars";
            var issuer = ConfigurationManager.AppSettings["Jwt:Issuer"] ?? "ecommerce-huit";
            var audience = ConfigurationManager.AppSettings["Jwt:Audience"] ?? "ecommerce-huit-client";
            var durationMinutes = 1440;
            int.TryParse(ConfigurationManager.AppSettings["Jwt:DurationInMinutes"], out durationMinutes);

            var expires = (long)(DateTime.UtcNow.AddMinutes(durationMinutes) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            // Create Header
            string headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));

            // Create Payload
            string payloadJson = string.Format(
                "{{\"sub\":\"{0}\",\"email\":\"{1}\",\"role\":\"{2}\",\"iss\":\"{3}\",\"aud\":\"{4}\",\"exp\":{5},\"jti\":\"{6}\"}}",
                EscapeJson(userId), EscapeJson(email), EscapeJson(role), EscapeJson(issuer), EscapeJson(audience), expires, Guid.NewGuid().ToString()
            );
            string payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            // Create Signature
            string unsignedToken = headerBase64 + "." + payloadBase64;
            byte[] signatureBytes;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keyStr)))
            {
                signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
            }
            string signatureBase64 = Base64UrlEncode(signatureBytes);

            return unsignedToken + "." + signatureBase64;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}

