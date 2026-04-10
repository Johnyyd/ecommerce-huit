using System;

namespace HuitShopDB.Models.DTOs.User
{
    public class UserDto
    {
        public UserDto()
        {
            FullName = string.Empty;
            Email = string.Empty;
            Role = string.Empty;
            Status = string.Empty;
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }
        public string Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        public UpdateProfileRequest()
        {
        }

        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
    }
}

