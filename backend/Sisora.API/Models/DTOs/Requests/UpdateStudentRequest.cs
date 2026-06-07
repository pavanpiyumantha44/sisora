namespace Sisora.API.Models.DTOs.Requests;

public class UpdateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string? PickupAddress { get; set; }
}