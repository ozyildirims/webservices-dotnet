using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;
using Microsoft.EntityFrameworkCore;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Repositories;

public interface IAnnouncementRepository
{
    Task<List<Announcement>> GetActiveAnnouncementsAsync(Guid userId, int skip = 0, int take = 10);
    Task<Announcement?> GetByIdAsync(Guid id);
    Task<Announcement> CreateAsync(Announcement announcement);
    Task<Announcement> UpdateAsync(Announcement announcement);
    Task DeleteAsync(Guid id);
    Task<List<UserNotification>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int skip = 0, int take = 10);
    Task<UserNotification> MarkNotificationAsReadAsync(Guid userId, Guid notificationId);
    Task<List<NotificationSetting>> GetUserNotificationSettingsAsync(Guid userId);
    Task UpdateNotificationSettingsAsync(Guid userId, Dictionary<string, bool> settings);
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

    public async Task<List<Announcement>> GetActiveAnnouncementsAsync(Guid userId, int skip = 0, int take = 10)
    {
        var now = DateTime.UtcNow;
        return await _context.Announcements
            .Include(a => a.Targets)
            .Where(a => a.IsActive && a.StartDate <= now && (!a.EndDate.HasValue || a.EndDate > now))
            .Where(a => !a.Targets.Any() || a.Targets.Any(t => 
                (t.TargetType == "User" && t.TargetId == userId) ||
                (t.TargetType == "Role" && _context.Users.Any(u => u.Id == userId && u.RoleId == t.TargetId))))
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Announcement?> GetByIdAsync(Guid id)
    {
        return await _context.Announcements
            .Include(a => a.Targets)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Announcement> CreateAsync(Announcement announcement)
    {
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();
        return announcement;
    }

    public async Task<Announcement> UpdateAsync(Announcement announcement)
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

    public async Task UpdateNotificationSettingsAsync(Guid userId, Dictionary<string, bool> settings)
    {
        var existingSettings = await _context.NotificationSettings
            .Where(s => s.UserId == userId)
            .ToListAsync();

        foreach (var setting in settings)
        {
            var existingSetting = existingSettings.FirstOrDefault(s => s.NotificationType == setting.Key);
            if (existingSetting != null)
            {
                existingSetting.IsEnabled = setting.Value;
                existingSetting.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                _context.NotificationSettings.Add(new NotificationSetting
                {
                    UserId = userId,
                    NotificationType = setting.Key,
                    IsEnabled = setting.Value,
                    CreatedAt = DateTime.UtcNow
                });
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
        return await _context.Users
            .Where(u => u.RoleId == roleId)
            .ToListAsync();
    }
} 