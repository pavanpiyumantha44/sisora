namespace Sisora.API.Models.Entities;

public class Student : BaseEntity
{
    public Guid ServiceRouteId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string? PickupAddress { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool InviteCodeUsed { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ServiceRoute ServiceRoute { get; set; } = null!;
    public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    public ICollection<TripEvent> TripEvents { get; set; } = new List<TripEvent>();
}