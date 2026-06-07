namespace Sisora.API.Models.Entities;

public class Parent : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FcmToken { get; set; }

    public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
}