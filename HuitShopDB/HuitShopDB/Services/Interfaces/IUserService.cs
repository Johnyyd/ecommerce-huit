using System.Collections.Generic;
using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.User;

namespace HuitShopDB.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetUsersAsync(string search, string role, string status);
        Task<UserDto> GetUserByIdAsync(int id);
        Task<bool> UpdateUserStatusAsync(int id, string status);
        Task<bool> UpdateUserRoleAsync(int id, string role);
        
        // New methods for enhanced features
        Task<UserDetailDto> GetUserDetailsAsync(int id);
        Task<bool> BulkUpdateUserStatusAsync(List<int> userIds, string status);
        Task<bool> BulkUpdateUserRoleAsync(List<int> userIds, string role);
        Task<IEnumerable<UserActivityDto>> GetUserActivitiesAsync(int userId);
        Task AddUserActivityAsync(int userId, string activityType, string description, string ipAddress = null);
    }
}
