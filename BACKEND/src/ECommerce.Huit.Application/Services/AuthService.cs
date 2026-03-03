using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Auth;
using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(ApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
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

        // Hash password (use BCrypt or similar)
        // For now: simple placeholder (NEVER store plain text!)
        var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(registerDto.Password));

        var user = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            Phone = registerDto.Phone,
            PasswordHash = passwordHash,
            Role = Domain.Enums.UserRole.CUSTOMER,
            Status = Domain.Enums.UserStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Save refresh token (in real app, use separate table or token store)
        // For now, we skip storing

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Status == Domain.Enums.UserStatus.ACTIVE);

        if (user == null)
            return null;

        // Verify password (compare hash)
        // This is a placeholder - use proper password hashing!
        var inputPasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(loginDto.Password));
        if (user.PasswordHash != inputPasswordHash)
            return null;

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        //TODO: Implement token revocation (store in DB or Redis blacklist)
        return Task.FromResult(true);
    }

    public Task<string?> RefreshAccessTokenAsync(string refreshToken)
    {
        var userId = _jwtTokenGenerator.ValidateRefreshToken(refreshToken);
        if (!userId.HasValue) return Task.FromResult<string?>(null);

        // TODO: Get user from DB and generate new access token
        // For now, return placeholder
        return Task.FromResult<string?>("new_access_token_here");
    }
}
