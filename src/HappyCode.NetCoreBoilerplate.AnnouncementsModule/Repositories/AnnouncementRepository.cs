using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;
using Microsoft.EntityFrameworkCore;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Repositories;

public interface IAnnouncementRepository
{
    Task<List<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement>> GetActiveAnnouncementsAsync(Guid userId, int skip = 0, int take = 10);
    Task<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement?> GetByIdAsync(Guid id);
    Task<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement> CreateAsync(HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement announcement);
    Task<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement> UpdateAsync(HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement announcement);
    Task DeleteAsync(Guid id);
    Task<List<UserNotification>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int skip = 0, int take = 10);
    Task<UserNotification> MarkNotificationAsReadAsync(Guid userId, Guid notificationId);
    Task<List<NotificationSetting>> GetUserNotificationSettingsAsync(Guid userId);
    Task UpdateNotificationSettingsAsync(Guid userId, List<NotificationSetting> settings);
    Task<UserDevice> RegisterDeviceAsync(Guid userId, string deviceToken, string deviceType);
    Task<List<UserDevice>> GetActiveUserDevicesAsync(Guid userId);
    Task<List<User>> GetUsersByRoleIdAsync(Guid roleId);
}

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly AnnouncementsContext _context;

    public AnnouncementRepository(AnnouncementsContext context)
    {
        _context = context;
    }

    public async Task<List<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement>> GetActiveAnnouncementsAsync(Guid userId, int skip = 0, int take = 10)
    {
        var now = DateTime.UtcNow;
        return await _context.Announcements
            .Include(a => a.Targets)
            .Where(a => a.IsActive && a.StartDate <= now && (!a.EndDate.HasValue || a.EndDate > now))
            .Where(a => !a.Targets.Any() || a.Targets.Any(t => 
                (t.TargetType == "User" && t.TargetId == userId) ||
                (t.TargetType == "Role" && true)))
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement?> GetByIdAsync(Guid id)
    {
        return await _context.Announcements
            .Include(a => a.Targets)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement> CreateAsync(HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement announcement)
    {
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();
        return announcement;
    }

    public async Task<HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement> UpdateAsync(HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models.Announcement announcement)
    {
        _context.Entry(announcement).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return announcement;
    }

    public async Task DeleteAsync(Guid id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement != null)
        {
            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<UserNotification>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int skip = 0, int take = 10)
    {
        var query = _context.UserNotifications
            .Include(n => n.Announcement)
            .Where(n => n.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<UserNotification> MarkNotificationAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            throw new InvalidOperationException("Notification not found");
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return notification;
    }

    public async Task<List<NotificationSetting>> GetUserNotificationSettingsAsync(Guid userId)
    {
        return await _context.NotificationSettings
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task UpdateNotificationSettingsAsync(Guid userId, List<NotificationSetting> settings)
    {
        var existingSettings = await _context.NotificationSettings
            .Where(s => s.UserId == userId)
            .ToListAsync();

        foreach (var setting in settings)
        {
            var existingSetting = existingSettings.FirstOrDefault(s => s.NotificationType == setting.NotificationType);
            if (existingSetting != null)
            {
                existingSetting.IsEnabled = setting.IsEnabled;
                existingSetting.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                _context.NotificationSettings.Add(setting);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserDevice> RegisterDeviceAsync(Guid userId, string deviceToken, string deviceType)
    {
        var existingDevice = await _context.UserDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceToken == deviceToken);

        if (existingDevice != null)
        {
            existingDevice.IsActive = true;
            existingDevice.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingDevice;
        }

        var device = new UserDevice
        {
            UserId = userId,
            DeviceToken = deviceToken,
            DeviceType = deviceType,
            IsActive = true,
            LastUsedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserDevices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<List<UserDevice>> GetActiveUserDevicesAsync(Guid userId)
    {
        return await _context.UserDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync();
    }

    public async Task<List<User>> GetUsersByRoleIdAsync(Guid roleId)
    {
        // TODO: Implement proper user lookup by role
        // For now, return empty list to avoid context issues
        return new List<User>();
    }
} 