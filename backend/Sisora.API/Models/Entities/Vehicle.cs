using backend.Models.Enums;

namespace Sisora.API.Models.Entities;

public class Vehicle : BaseEntity
{
    public Guid DriverId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Model { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? VinNumber { get; set; }
    public string? Color { get; set; }
    public bool IsPrimary { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public Driver Driver { get; set; } = null!;
}