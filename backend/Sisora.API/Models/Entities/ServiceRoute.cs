namespace Sisora.API.Models.Entities;

public class ServiceRoute : BaseEntity
{
    public Guid DriverId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AreaDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Driver Driver { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}