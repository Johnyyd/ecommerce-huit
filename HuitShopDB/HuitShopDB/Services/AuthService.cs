using System;
using System.Linq;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Auth;
using HuitShopDB.Models;

namespace HuitShopDB.Services
{
    public class AuthService : IAuthService
    {
        private readonly HuitShopDBDataContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService()
        {
            _context = new HuitShopDBDataContext();
            _jwtTokenGenerator = new JwtTokenGenerator();
        }

        public AuthService(IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = new HuitShopDBDataContext();
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public AuthResponseDto Register(RegisterDto registerDto)
        {
            // Check if email exists
            var existingUser = _context.users
                .FirstOrDefault(u => u.email == registerDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email đã được sử dụng");

            // Check if phone exists
            if (!string.IsNullOrEmpty(registerDto.Phone))
            {
                var existingPhone = _context.users
                    .FirstOrDefault(u => u.phone == registerDto.Phone);
                if (existingPhone != null)
                    throw new InvalidOperationException("Số điện thoại đã được sử dụng");
            }

            // NOTE: Dev only – store plain text password
            var passwordHash = registerDto.Password;

            var user = new user();
            user.full_name = registerDto.FullName;
            user.email = registerDto.Email;
            user.phone = registerDto.Phone;
            user.password_hash = passwordHash;
            user.role = "CUSTOMER";
            user.status = "ACTIVE";
            user.created_at = DateTime.UtcNow;

            _context.users.InsertOnSubmit(user);
            _context.SubmitChanges();

            // Generate tokens
            var accessToken = _jwtTokenGenerator.GenerateToken(user.id.ToString(), user.email, user.role);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            var response = new AuthResponseDto();
            response.Id = user.id;
            response.Email = user.email;
            response.FullName = user.full_name;
            response.Role = user.role;
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;

            return response;
        }

        public AuthResponseDto Login(LoginDto loginDto)
        {
            var user = _context.users
                .FirstOrDefault(u => u.email == loginDto.Email && u.status == "ACTIVE");

            if (user == null)
                return null;

            // Dev only: compare plain text passwords
            if (user.password_hash != loginDto.Password)
                return null;

            user.last_login = DateTime.UtcNow;
            _context.SubmitChanges();

            var accessToken = _jwtTokenGenerator.GenerateToken(user.id.ToString(), user.email, user.role);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            var response = new AuthResponseDto();
            response.Id = user.id;
            response.Email = user.email;
            response.FullName = user.full_name;
            response.Role = user.role;
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;

            return response;
        }

        public bool RevokeRefreshToken(string refreshToken)
        {
            return true;
        }

        public string RefreshAccessToken(string refreshToken)
        {
            // Simplified for migration
            return "new_access_token_here";
        }
    }
}



