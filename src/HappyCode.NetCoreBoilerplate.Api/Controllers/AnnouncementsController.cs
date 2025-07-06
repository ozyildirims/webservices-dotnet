using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyCode.NetCoreBoilerplate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnnouncementsController : ApiControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAnnouncements(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var announcements = await _announcementService.GetActiveAnnouncementsAsync(UserId, skip, take);
        return Ok(announcements);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnnouncementDto>> GetAnnouncement(Guid id)
    {
        var announcement = await _announcementService.GetByIdAsync(id);
        return Ok(announcement);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AnnouncementDto>> CreateAnnouncement([FromBody] CreateAnnouncementDto dto)
    {
        var announcement = await _announcementService.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetAnnouncement), new { id = announcement.Id }, announcement);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AnnouncementDto>> UpdateAnnouncement(Guid id, [FromBody] UpdateAnnouncementDto dto)
    {
        var announcement = await _announcementService.UpdateAsync(UserId, id, dto);
        return Ok(announcement);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteAnnouncement(Guid id)
    {
        await _announcementService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<List<UserNotificationDto>>> GetNotifications(
        [FromQuery] bool? isRead = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var notifications = await _announcementService.GetUserNotificationsAsync(UserId, isRead, skip, take);
        return Ok(notifications);
    }

    [HttpPost("notifications/{id:guid}/read")]
    public async Task<ActionResult<UserNotificationDto>> MarkNotificationAsRead(Guid id)
    {
        var notification = await _announcementService.MarkNotificationAsReadAsync(UserId, id);
        return Ok(notification);
    }

    [HttpGet("settings")]
    public async Task<ActionResult<List<NotificationSettingDto>>> GetNotificationSettings()
    {
        var settings = await _announcementService.GetUserNotificationSettingsAsync(UserId);
        return Ok(settings);
    }

    [HttpPut("settings")]
    public async Task<ActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsDto dto)
    {
        await _announcementService.UpdateNotificationSettingsAsync(UserId, dto);
        return NoContent();
    }

    [HttpPost("devices")]
    public async Task<ActionResult<UserDeviceDto>> RegisterDevice([FromBody] RegisterDeviceDto dto)
    {
        var device = await _announcementService.RegisterDeviceAsync(UserId, dto);
        return Ok(device);
    }
} 