namespace Sisora.API.Models.Entities;

using backend.Models.Enums;
using Sisora.API.Models.Enums;

public class Trip : BaseEntity
{
    public Guid ServiceRouteId { get; set; }
    public TripType TripType { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    public ServiceRoute ServiceRoute { get; set; } = null!;
    public ICollection<TripEvent> TripEvents { get; set; } = new List<TripEvent>();
}