using System;
using System.Collections.Generic;
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
            Addresses = new List<AddressesDto>();
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
        public List<AddressesDto> Addresses { get; set; }
    }

    public class AddressesDto
    {
        public string Label { get; set; }
        public string FullAddress { get; set; }
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

