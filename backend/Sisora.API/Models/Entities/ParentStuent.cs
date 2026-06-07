namespace Sisora.API.Models.Entities;

public class ParentStudent
{
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    public Parent Parent { get; set; } = null!;
    public Student Student { get; set; } = null!;
}