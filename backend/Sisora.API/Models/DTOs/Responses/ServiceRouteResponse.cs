namespace Sisora.API.Models.DTOs.Responses;

public class ServiceRouteResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AreaDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}