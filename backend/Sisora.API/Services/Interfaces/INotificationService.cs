namespace Sisora.API.Services.Interfaces;

public interface INotificationService
{
    Task SendToTokenAsync(string fcmToken, string title, string body);
    Task SendToMultipleAsync(List<string> fcmTokens, string title, string body);
}