using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Repositories;
using HappyCode.NetCoreBoilerplate.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Services;

public interface IAnnouncementService
{
    Task<List<AnnouncementDto>> GetActiveAnnouncementsAsync(Guid userId, int skip = 0, int take = 10);
    Task<AnnouncementDto> GetByIdAsync(Guid id);
    Task<AnnouncementDto> CreateAsync(Guid createdBy, CreateAnnouncementDto dto);
    Task<AnnouncementDto> UpdateAsync(Guid modifiedBy, Guid id, UpdateAnnouncementDto dto);
    Task DeleteAsync(Guid id);
    Task<List<UserNotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int skip = 0, int take = 10);
    Task<UserNotificationDto> MarkNotificationAsReadAsync(Guid userId, Guid notificationId);
    Task<List<NotificationSettingDto>> GetUserNotificationSettingsAsync(Guid userId);
    Task UpdateNotificationSettingsAsync(Guid userId, UpdateNotificationSettingsDto dto);
    Task<UserDeviceDto> RegisterDeviceAsync(Guid userId, RegisterDeviceDto dto);
}

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AnnouncementService> _logger;

    public AnnouncementService(
        IAnnouncementRepository repository,
        INotificationService notificationService,
        ILogger<AnnouncementService> logger)
    {
        _repository = repository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<List<AnnouncementDto>> GetActiveAnnouncementsAsync(Guid userId, int skip = 0, int take = 10)
    {
        var announcements = await _repository.GetActiveAnnouncementsAsync(userId, skip, take);
        return announcements.Select(ToDto).ToList();
    }

    public async Task<AnnouncementDto> GetByIdAsync(Guid id)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement == null)
        {
            throw new InvalidOperationException($"Announcement with ID {id} not found");
        }
        return ToDto(announcement);
    }

    public async Task<AnnouncementDto> CreateAsync(Guid createdBy, CreateAnnouncementDto dto)
    {
        var announcement = new HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement
        {
            Title = dto.Title,
            Content = dto.Content,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Priority = dto.Priority,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            Targets = dto.Targets.Select(t => new AnnouncementTarget
            {
                TargetType = t.TargetType,
                TargetId = t.TargetId
            }).ToList()
        };

        announcement = await _repository.CreateAsync(announcement);

        // Send notifications to targeted users
        var userIds = await GetTargetedUserIds(announcement);
        await _notificationService.SendAnnouncementNotificationAsync(announcement, userIds);

        return ToDto(announcement);
    }

    public async Task<AnnouncementDto> UpdateAsync(Guid modifiedBy, Guid id, UpdateAnnouncementDto dto)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement == null)
        {
            throw new InvalidOperationException($"Announcement with ID {id} not found");
        }

        announcement.Title = dto.Title;
        announcement.Content = dto.Content;
        announcement.StartDate = dto.StartDate;
        announcement.EndDate = dto.EndDate;
        announcement.IsActive = dto.IsActive;
        announcement.Priority = dto.Priority;
        announcement.ModifiedBy = modifiedBy;
        announcement.ModifiedAt = DateTime.UtcNow;

        // Update targets
        announcement.Targets.Clear();
        foreach (var target in dto.Targets)
        {
            announcement.Targets.Add(new AnnouncementTarget
            {
                TargetType = target.TargetType,
                TargetId = target.TargetId
            });
        }

        announcement = await _repository.UpdateAsync(announcement);
        return ToDto(announcement);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<List<UserNotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int skip = 0, int take = 10)
    {
        var notifications = await _repository.GetUserNotificationsAsync(userId, isRead, skip, take);
        return notifications.Select(ToDto).ToList();
    }

    public async Task<UserNotificationDto> MarkNotificationAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _repository.MarkNotificationAsReadAsync(userId, notificationId);
        return ToDto(notification);
    }

    public async Task<List<NotificationSettingDto>> GetUserNotificationSettingsAsync(Guid userId)
    {
        var settings = await _repository.GetUserNotificationSettingsAsync(userId);
        return settings.Select(ToDto).ToList();
    }

    public async Task UpdateNotificationSettingsAsync(Guid userId, UpdateNotificationSettingsDto dto)
    {
        var settings = dto.Settings.Select(s => new NotificationSetting
        {
            UserId = userId,
            NotificationType = s.Key,
            IsEnabled = s.Value,
            CreatedAt = DateTime.UtcNow
        }).ToList();
        
        await _repository.UpdateNotificationSettingsAsync(userId, settings);
    }

    public async Task<UserDeviceDto> RegisterDeviceAsync(Guid userId, RegisterDeviceDto dto)
    {
        var device = await _repository.RegisterDeviceAsync(userId, dto.DeviceToken, dto.DeviceType);
        return ToDto(device);
    }

    private async Task<List<Guid>> GetTargetedUserIds(HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement announcement)
    {
        var userIds = new HashSet<Guid>();

        foreach (var target in announcement.Targets)
        {
            if (target.TargetType == "User")
            {
                userIds.Add(target.TargetId);
            }
            else if (target.TargetType == "Role")
            {
                // Get all users with the targeted role
                var usersWithRole = await _repository.GetUsersByRoleIdAsync(target.TargetId);
                foreach (var user in usersWithRole)
                {
                    userIds.Add(Guid.Parse(user.Id.ToString()));
                }
            }
        }

        return userIds.ToList();
    }

    private static AnnouncementDto ToDto(HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement announcement) => new()
    {
        Id = announcement.Id,
        Title = announcement.Title,
        Content = announcement.Content,
        StartDate = announcement.StartDate,
        EndDate = announcement.EndDate,
        IsActive = announcement.IsActive,
        Priority = announcement.Priority,
        CreatedBy = announcement.CreatedBy,
        CreatedAt = announcement.CreatedAt,
        ModifiedBy = announcement.ModifiedBy,
        ModifiedAt = announcement.ModifiedAt,
        Targets = announcement.Targets.Select(t => new AnnouncementTargetDto
        {
            Id = t.Id,
            TargetType = t.TargetType,
            TargetId = t.TargetId
        }).ToList()
    };

    private static UserNotificationDto ToDto(UserNotification notification) => new()
    {
        Id = notification.Id,
        UserId = notification.UserId,
        Announcement = ToDto(notification.Announcement),
        IsRead = notification.IsRead,
        ReadAt = notification.ReadAt,
        CreatedAt = notification.CreatedAt
    };

    private static NotificationSettingDto ToDto(NotificationSetting setting) => new()
    {
        Id = setting.Id,
        UserId = setting.UserId,
        NotificationType = setting.NotificationType,
        IsEnabled = setting.IsEnabled,
        CreatedAt = setting.CreatedAt,
        ModifiedAt = setting.ModifiedAt
    };

    private static UserDeviceDto ToDto(UserDevice device) => new()
    {
        Id = device.Id,
        UserId = device.UserId,
        DeviceToken = device.DeviceToken,
        DeviceType = device.DeviceType,
        IsActive = device.IsActive,
        LastUsedAt = device.LastUsedAt,
        CreatedAt = device.CreatedAt
    };
} 