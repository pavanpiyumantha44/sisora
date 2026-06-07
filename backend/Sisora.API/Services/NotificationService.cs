using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Services;

public class NotificationService : INotificationService
{
    public NotificationService()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var credential = GoogleCredential
                .FromFile("firebase-adminsdk.json");

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }
    }

    public async Task SendToTokenAsync(string fcmToken, string title, string body)
    {
        var message = new Message
        {
            Token = fcmToken,
            Notification = new Notification { Title = title, Body = body },
            Webpush = new WebpushConfig
            {
                Notification = new WebpushNotification
                {
                    Title = title,
                    Body = body,
                    Icon = "/icon-192x192.png"
                }
            }
        };

        try
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FCM send error: {ex.Message}");
        }
    }

    public async Task SendToMultipleAsync(List<string> fcmTokens, string title, string body)
    {
        if (!fcmTokens.Any()) return;

        var tasks = fcmTokens
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(token => SendToTokenAsync(token, title, body));

        await Task.WhenAll(tasks);
    }
}