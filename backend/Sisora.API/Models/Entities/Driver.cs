using Sisora.API.Models.Enums;

namespace Sisora.API.Models.Entities;

public class Driver : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NIC { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DriverStatus Status { get; set; } = DriverStatus.Pending;
    public string? RejectionReason { get; set; }
    public string? FcmToken { get; set; }

    public ICollection<ServiceRoute> ServiceRoutes { get; set; } = new List<ServiceRoute>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}