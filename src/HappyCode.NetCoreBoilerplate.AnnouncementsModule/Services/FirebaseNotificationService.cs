using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Services;

public interface INotificationService
{
    Task SendAnnouncementNotificationAsync(Announcement announcement, List<Guid> userIds);
    Task SendCustomNotificationAsync(string title, string body, List<Guid> userIds, Dictionary<string, string>? data = null);
}

public class FirebaseNotificationService : INotificationService
{
    private readonly IAnnouncementRepository _repository;
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly FirebaseMessaging _messaging;

    public FirebaseNotificationService(
        IAnnouncementRepository repository,
        ILogger<FirebaseNotificationService> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;

        if (FirebaseApp.DefaultInstance == null)
        {
            var credentialsJson = configuration["Firebase:CredentialsJson"];
            if (string.IsNullOrEmpty(credentialsJson))
            {
                throw new InvalidOperationException("Firebase credentials not found in configuration");
            }

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(credentialsJson)
            });
        }

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task SendAnnouncementNotificationAsync(Announcement announcement, List<Guid> userIds)
    {
        try
        {
            var devices = new List<UserDevice>();
            foreach (var userId in userIds)
            {
                var userDevices = await _repository.GetActiveUserDevicesAsync(userId);
                devices.AddRange(userDevices);
            }

            if (!devices.Any())
            {
                _logger.LogInformation("No active devices found for users");
                return;
            }

            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = announcement.Title,
                    Body = announcement.Content
                },
                Data = new Dictionary<string, string>
                {
                    { "announcementId", announcement.Id.ToString() },
                    { "type", "announcement" }
                },
                Tokens = devices.Select(d => d.DeviceToken).ToList()
            };

            var response = await _messaging.SendMulticastAsync(message);
            _logger.LogInformation(
                "Sent announcement notification to {SuccessCount} devices, failed: {FailureCount}",
                response.SuccessCount,
                response.FailureCount);

            // Handle failed tokens
            if (response.FailureCount > 0)
            {
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        var failedToken = devices[i].DeviceToken;
                        _logger.LogWarning(
                            "Failed to send notification to token: {Token}, Error: {Error}",
                            failedToken,
                            response.Responses[i].Exception?.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending announcement notification");
            throw;
        }
    }

    public async Task SendCustomNotificationAsync(string title, string body, List<Guid> userIds, Dictionary<string, string>? data = null)
    {
        try
        {
            var devices = new List<UserDevice>();
            foreach (var userId in userIds)
            {
                var userDevices = await _repository.GetActiveUserDevicesAsync(userId);
                devices.AddRange(userDevices);
            }

            if (!devices.Any())
            {
                _logger.LogInformation("No active devices found for users");
                return;
            }

            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Tokens = devices.Select(d => d.DeviceToken).ToList()
            };

            var response = await _messaging.SendMulticastAsync(message);
            _logger.LogInformation(
                "Sent custom notification to {SuccessCount} devices, failed: {FailureCount}",
                response.SuccessCount,
                response.FailureCount);

            // Handle failed tokens
            if (response.FailureCount > 0)
            {
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        var failedToken = devices[i].DeviceToken;
                        _logger.LogWarning(
                            "Failed to send notification to token: {Token}, Error: {Error}",
                            failedToken,
                            response.Responses[i].Exception?.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom notification");
            throw;
        }
    }
} 