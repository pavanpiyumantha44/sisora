using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;

namespace Sisora.API.Services.Interfaces;

public interface IDriverService
{
    // routes
    Task<ApiResponse<ServiceRouteResponse>> CreateRouteAsync(Guid driverId, CreateServiceRouteRequest request);
    Task<ApiResponse<List<ServiceRouteResponse>>> GetMyRoutesAsync(Guid driverId);
    Task<ApiResponse<ServiceRouteResponse>> UpdateRouteAsync(Guid driverId, Guid routeId, UpdateServiceRouteRequest request);
    Task<ApiResponse<bool>> DeleteRouteAsync(Guid driverId, Guid routeId);

    // students
    Task<ApiResponse<StudentResponse>> AddStudentAsync(Guid driverId, Guid routeId, AddStudentRequest request);
    Task<ApiResponse<List<StudentResponse>>> GetStudentsAsync(Guid driverId, Guid routeId);
    Task<ApiResponse<StudentResponse>> UpdateStudentAsync(Guid driverId, Guid routeId, Guid studentId, UpdateStudentRequest request);
    Task<ApiResponse<bool>> RemoveStudentAsync(Guid driverId, Guid routeId, Guid studentId);

    // invite codes
    Task<ApiResponse<InviteCodeResponse>> RegenerateInviteCodeAsync(Guid driverId, Guid routeId, Guid studentId);
}