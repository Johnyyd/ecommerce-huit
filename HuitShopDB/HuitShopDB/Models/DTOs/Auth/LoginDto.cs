using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HuitShopDB.Models.DTOs.Auth
{
    public class LoginDto
    {
        public LoginDto()
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        [Required]
        [EmailAddress]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}

