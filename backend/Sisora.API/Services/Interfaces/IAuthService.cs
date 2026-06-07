using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;

namespace Sisora.API.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterDriverAsync(RegisterDriverRequest request);
    Task<ApiResponse<AuthResponse>> RegisterParentAsync(RegisterParentRequest request);
    Task<ApiResponse<AuthResponse>> LoginDriverAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> LoginParentAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> LoginAdminAsync(AdminLoginRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> RegisterFcmTokenAsync(string userId, string role, string token);
}