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
    }
}
