namespace Sisora.API.Models.DTOs.Requests;

public class RegisterParentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}