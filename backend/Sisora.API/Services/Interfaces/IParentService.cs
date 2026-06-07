using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;

namespace Sisora.API.Services.Interfaces;

public interface IParentService
{
    Task<ApiResponse<StudentResponse>> RedeemInviteCodeAsync(Guid parentId, RedeemInviteCodeRequest request);
    Task<ApiResponse<List<StudentResponse>>> GetMyChildrenAsync(Guid parentId);
}