namespace Sisora.API.Models.DTOs.Requests;

public class TripEventRequest
{
    public Guid StudentId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}