using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Sisora.API.Data;
using Sisora.API.Helpers;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;
using Sisora.API.Models.Entities;
using Sisora.API.Models.Enums;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, JwtHelper jwtHelper, IConfiguration configuration)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _configuration = configuration;
    }

    // ── Driver Register ───────────────────────────────────
    public async Task<ApiResponse<AuthResponse>> RegisterDriverAsync(RegisterDriverRequest request)
    {
        // check phone already exists
        var exists = await _context.Drivers
            .AnyAsync(d => d.Phone == request.Phone);

        if (exists)
            return ApiResponse<AuthResponse>.Fail("Phone number already registered.");

        // parse vehicle type
        if (!Enum.TryParse<VehicleType>(request.VehicleType, ignoreCase: true, out var vehicleType))
            return ApiResponse<AuthResponse>.Fail("Invalid vehicle type.");

        // check registration number already exists
        var regExists = await _context.Vehicles
            .AnyAsync(v => v.RegistrationNumber == request.RegistrationNumber);

        if (regExists)
            return ApiResponse<AuthResponse>.Fail("Vehicle registration number already registered.");

        var driver = new Driver
        {
            FullName = request.FullName,
            Phone = request.Phone,
            NIC = request.NIC,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Status = DriverStatus.Pending
        };

        var vehicle = new Vehicle
        {
            DriverId = driver.Id,
            Model = request.VehicleModel,
            RegistrationNumber = request.RegistrationNumber,
            VinNumber = request.VinNumber,
            Color = request.VehicleColor,
            VehicleType = vehicleType,
            IsPrimary = true
        };

        await _context.Drivers.AddAsync(driver);
        await _context.Vehicles.AddAsync(vehicle);
        await _context.SaveChangesAsync();

        // driver is pending — no token yet, just confirm registration
        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            UserId = driver.Id,
            FullName = driver.FullName,
            Role = "Driver"
        }, "Registration successful. Please wait for admin approval.");
    }

    // ── Parent Register ───────────────────────────────────
    public async Task<ApiResponse<AuthResponse>> RegisterParentAsync(RegisterParentRequest request)
    {
        var exists = await _context.Parents
            .AnyAsync(p => p.Phone == request.Phone);

        if (exists)
            return ApiResponse<AuthResponse>.Fail("Phone number already registered.");

        var parent = new Parent
        {
            FullName = request.FullName,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _context.Parents.AddAsync(parent);
        await _context.SaveChangesAsync();

        var (accessToken, refreshToken) = await GenerateAndStoreTokensAsync(
            parent.Id.ToString(), "Parent", parent.FullName);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Role = "Parent",
            FullName = parent.FullName,
            UserId = parent.Id
        });
    }

    // ── Driver Login ──────────────────────────────────────
    public async Task<ApiResponse<AuthResponse>> LoginDriverAsync(LoginRequest request)
    {
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.Phone == request.Phone);

        if (driver == null || !BCrypt.Net.BCrypt.Verify(request.Password, driver.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid phone number or password.");

        if (driver.Status == DriverStatus.Pending)
            return ApiResponse<AuthResponse>.Fail("Your account is pending approval.");

        if (driver.Status == DriverStatus.NICVerified)
            return ApiResponse<AuthResponse>.Fail("Your account is pending vehicle verification.");

        if (driver.Status == DriverStatus.Rejected)
            return ApiResponse<AuthResponse>.Fail($"Your account has been rejected. Reason: {driver.RejectionReason}");

        if (driver.Status == DriverStatus.Suspended)
            return ApiResponse<AuthResponse>.Fail("Your account has been suspended. Please contact support.");

        var (accessToken, refreshToken) = await GenerateAndStoreTokensAsync(
            driver.Id.ToString(), "Driver", driver.FullName);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Role = "Driver",
            FullName = driver.FullName,
            UserId = driver.Id
        });
    }

    // ── Parent Login ──────────────────────────────────────
    public async Task<ApiResponse<AuthResponse>> LoginParentAsync(LoginRequest request)
    {
        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.Phone == request.Phone);

        if (parent == null || !BCrypt.Net.BCrypt.Verify(request.Password, parent.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid phone number or password.");

        var (accessToken, refreshToken) = await GenerateAndStoreTokensAsync(
            parent.Id.ToString(), "Parent", parent.FullName);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Role = "Parent",
            FullName = parent.FullName,
            UserId = parent.Id
        });
    }

    // ── Admin Login ───────────────────────────────────────
    public async Task<ApiResponse<AuthResponse>> LoginAdminAsync(AdminLoginRequest request)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");

        var (accessToken, refreshToken) = await GenerateAndStoreTokensAsync(
            admin.Id.ToString(), "Admin", admin.FullName);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Role = "Admin",
            FullName = admin.FullName,
            UserId = admin.Id
        });
    }

    // ── Refresh Token ─────────────────────────────────────
    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
            return ApiResponse<AuthResponse>.Fail("Invalid refresh token.");

        if (storedToken.IsRevoked)
            return ApiResponse<AuthResponse>.Fail("Refresh token has been revoked.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponse>.Fail("Refresh token has expired.");

        // revoke old token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        // resolve user name for new token
        var fullName = await GetFullNameAsync(storedToken.UserId, storedToken.Role);

        // issue new tokens
        var (accessToken, newRefreshToken) = await GenerateAndStoreTokensAsync(
            storedToken.UserId, storedToken.Role, fullName);

        await _context.SaveChangesAsync();

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            Role = storedToken.Role,
            FullName = fullName
        });
    }

    // ── Revoke Token ──────────────────────────────────────
    public async Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
            return ApiResponse<bool>.Fail("Token not found.");

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Token revoked successfully.");
    }

    // ── Private helpers ───────────────────────────────────
    private async Task<(string accessToken, string refreshToken)> GenerateAndStoreTokensAsync(
        string userId, string role, string fullName)
    {
        var accessToken = _jwtHelper.GenerateAccessToken(userId, role, fullName);
        var refreshToken = _jwtHelper.GenerateRefreshToken();
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            Role = role,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays)
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

    private async Task<string> GetFullNameAsync(string userId, string role)
    {
        return role switch
        {
            "Driver" => (await _context.Drivers.FindAsync(Guid.Parse(userId)))?.FullName ?? string.Empty,
            "Parent" => (await _context.Parents.FindAsync(Guid.Parse(userId)))?.FullName ?? string.Empty,
            "Admin"  => (await _context.Admins.FindAsync(Guid.Parse(userId)))?.FullName ?? string.Empty,
            _ => string.Empty
        };
    }
    public async Task<ApiResponse<bool>> RegisterFcmTokenAsync(string userId, string role, string token)
    {
        switch (role)
        {
            case "Driver":
                var driver = await _context.Drivers.FindAsync(Guid.Parse(userId));
                if (driver == null) return ApiResponse<bool>.Fail("Driver not found.");
                driver.FcmToken = token;
                break;

            case "Parent":
                var parent = await _context.Parents.FindAsync(Guid.Parse(userId));
                if (parent == null) return ApiResponse<bool>.Fail("Parent not found.");
                parent.FcmToken = token;
                break;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "FCM token registered.");
    }
}