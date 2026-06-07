namespace Sisora.API.Models.DTOs.Requests;

public class RegisterDriverRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NIC { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // vehicle details required at registration
    public string VehicleModel { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? VinNumber { get; set; }
    public string? VehicleColor { get; set; }
    public string VehicleType { get; set; } = string.Empty;
}