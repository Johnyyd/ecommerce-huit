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
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
    }

    /// <summary>
    /// Extended user information for admin management
    /// </summary>
    public class UserDetailDto
    {
        public UserDetailDto()
        {
            RecentActivities = new List<UserActivityDto>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<UserActivityDto> RecentActivities { get; set; }
    }

    /// <summary>
    /// User login/activity tracking
    /// </summary>
    public class UserActivityDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActivityType { get; set; } // LOGIN, PURCHASE, REVIEW, etc.
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Bulk user operations request
    /// </summary>
    public class BulkUserOperationRequest
    {
        public BulkUserOperationRequest()
        {
            UserIds = new List<int>();
        }

        public List<int> UserIds { get; set; }
        public string Operation { get; set; } // BAN, UNBAN, CHANGE_ROLE, DELETE
        public string NewRole { get; set; } // For CHANGE_ROLE operation
    }

    /// <summary>
    /// User export request
    /// </summary>
    public class UserExportRequest
    {
        public UserExportRequest()
        {
            Columns = new List<string>();
        }

        public string Format { get; set; } // CSV, EXCEL
        public List<string> Columns { get; set; } // Fields to export
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class UserExportDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
