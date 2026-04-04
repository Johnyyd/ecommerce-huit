using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Auth;
using ECommerce.Huit.Domain.Entities;

namespace ECommerce.Huit.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email đã được sử dụng");

            // Check if phone exists
            if (!string.IsNullOrEmpty(registerDto.Phone))
            {
                var existingPhone = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == registerDto.Phone);
                if (existingPhone != null)
                    throw new InvalidOperationException("Số điện thoại đã được sử dụng");
            }

            // NOTE: Dev only – store plain text password
            var passwordHash = registerDto.Password;

            var user = new User();
            user.FullName = registerDto.FullName;
            user.Email = registerDto.Email;
            user.Phone = registerDto.Phone;
            user.PasswordHash = passwordHash;
            user.Role = Domain.Enums.UserRole.CUSTOMER;
            user.Status = Domain.Enums.UserStatus.ACTIVE;
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = _jwtTokenGenerator.GenerateToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            var response = new AuthResponseDto();
            response.Id = user.Id;
            response.Email = user.Email;
            response.FullName = user.FullName;
            response.Role = user.Role.ToString();
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;

            return response;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Status == Domain.Enums.UserStatus.ACTIVE);

            if (user == null)
                return null;

            // Dev only: compare plain text passwords
            if (user.PasswordHash != loginDto.Password)
                return null;

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken = _jwtTokenGenerator.GenerateToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            var response = new AuthResponseDto();
            response.Id = user.Id;
            response.Email = user.Email;
            response.FullName = user.FullName;
            response.Role = user.Role.ToString();
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;

            return response;
        }

        public Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            return Task.FromResult(true);
        }

        public Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            // Simplified for migration
            return Task.FromResult("new_access_token_here");
        }
    }
}
