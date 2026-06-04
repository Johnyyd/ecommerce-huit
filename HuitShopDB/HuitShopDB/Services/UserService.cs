using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<UserDto> GetUsers(string search, string role, string status)
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

            return result;
        }

        public UserDto GetUserById(int id)
        {
            var user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return null;

            return MapToDto(user);
        }

        public bool UpdateUserStatus(int id, string status)
        {
            var user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return false;

            user.status = status;
            user.updated_at = DateTime.Now;
            _context.SubmitChanges();

            return true;
        }

        public bool UpdateUserRole(int id, string role)
        {
            var user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return false;

            user.role = role;
            user.updated_at = DateTime.Now;
            _context.SubmitChanges();

            return true;
        }

        public UserDetailDto GetUserDetails(int id)
        {
            var user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return null;

            var orders = _context.orders.Where(o => o.user_id == id).ToList();
            var totalSpent = orders.Sum(o => o.total);

            var detail = new UserDetailDto
            {
                Id = user.id,
                FullName = user.full_name,
                Email = user.email,
                Phone = user.phone,
                Role = user.role,
                Status = user.status,
                AvatarUrl = user.avatar_url,
                LastLogin = user.last_login ?? DateTime.MinValue,
                CreatedAt = user.created_at,
                UpdatedAt = user.updated_at,
                TotalOrders = orders.Count,
                TotalSpent = totalSpent
            };

            return detail;
        }

        public bool BulkUpdateUserStatus(List<int> userIds, string status)
        {
            var users = _context.users.Where(u => userIds.Contains(u.id)).ToList();
            foreach (var user in users)
            {
                user.status = status;
                user.updated_at = DateTime.Now;
            }
            _context.SubmitChanges();
            return true;
        }

        public bool BulkUpdateUserRole(List<int> userIds, string role)
        {
            var users = _context.users.Where(u => userIds.Contains(u.id)).ToList();
            foreach (var user in users)
            {
                user.role = role;
                user.updated_at = DateTime.Now;
            }
            _context.SubmitChanges();
            return true;
        }

        public IEnumerable<UserActivityDto> GetUserActivities(int userId)
        {
            // This assumes there's an audit_log or user_activity table
            // For now, return a simple implementation
            return new List<UserActivityDto>();
        }

        public void AddUserActivity(int userId, string activityType, string description, string ipAddress = null)
        {
            // This would log user activities for tracking purposes
            // Implementation depends on your audit log table structure

        }

        public bool UpdateUserProfile(int userId, string fullName, string phone, string avatarUrl)
        {
            var user = _context.users.FirstOrDefault(u => u.id == userId);
            if (user == null) return false;

            user.full_name = fullName;
            user.phone = phone;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                user.avatar_url = avatarUrl;
            }
            user.updated_at = DateTime.Now;

            _context.SubmitChanges();
            return true;
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


