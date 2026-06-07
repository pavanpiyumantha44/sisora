namespace Sisora.API.Models.DTOs.Responses;

public class StudentResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string? PickupAddress { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool InviteCodeUsed { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}