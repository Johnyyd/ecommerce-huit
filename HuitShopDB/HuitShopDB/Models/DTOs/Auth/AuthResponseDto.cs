using System;
using Newtonsoft.Json;

namespace HuitShopDB.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public AuthResponseDto()
        {
            Email = string.Empty;
            FullName = string.Empty;
            Role = string.Empty;
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
        }

        public int Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}

