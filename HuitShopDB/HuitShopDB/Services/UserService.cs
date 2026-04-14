using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Models;
using HuitShopDB.Models.DTOs.User;
using HuitShopDB.Services.Interfaces;

namespace HuitShopDB.Services
{
    public class UserService : IUserService
    {
        private readonly HuitShopDBDataContext _context;

        public UserService()
        {
            _context = new HuitShopDBDataContext();
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(string search, string role, string status)
        {
            var query = _context.users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.full_name.Contains(search) || u.email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.role == role);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.status == status);
            }

            var users = query.OrderByDescending(u => u.created_at).ToList();
            var result = users.Select(u => MapToDto(u)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return null;

            return await Task.FromResult(MapToDto(user));
        }

        public async Task<bool> UpdateUserStatusAsync(int id, string status)
        {
            var user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return false;

            user.status = status;
            user.updated_at = DateTime.Now;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateUserRoleAsync(int id, string role)
        {
            var user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return false;

            user.role = role;
            user.updated_at = DateTime.Now;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        private UserDto MapToDto(user u)
        {
            return new UserDto
            {
                Id = u.id,
                FullName = u.full_name,
                Email = u.email,
                Phone = u.phone,
                Role = u.role,
                Status = u.status,
                AvatarUrl = u.avatar_url,
                LastLogin = u.last_login,
                CreatedAt = u.created_at
            };
        }
    }
}
