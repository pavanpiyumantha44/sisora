namespace Sisora.API.Models.DTOs.Requests;

public class AddStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string? PickupAddress { get; set; }
}