using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ECommerce.Huit.Application.DTOs.Auth
{
    public class RegisterDto
    {
        public RegisterDto()
        {
            FullName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }

        [Required]
        [StringLength(100)]
        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }
}
