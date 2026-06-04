using System.Collections.Generic;
using HuitShopDB.Models.DTOs.User;

namespace HuitShopDB.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserDto> GetUsers(string search, string role, string status);
        UserDto GetUserById(int id);
        bool UpdateUserStatus(int id, string status);
        bool UpdateUserRole(int id, string role);
        
        // New methods for enhanced features
        UserDetailDto GetUserDetails(int id);
        bool BulkUpdateUserStatus(List<int> userIds, string status);
        bool BulkUpdateUserRole(List<int> userIds, string role);
        IEnumerable<UserActivityDto> GetUserActivities(int userId);
        void AddUserActivity(int userId, string activityType, string description, string ipAddress = null);
        bool UpdateUserProfile(int userId, string fullName, string phone, string avatarUrl);
    }
}

