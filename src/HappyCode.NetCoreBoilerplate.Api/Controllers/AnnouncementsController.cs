using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyCode.NetCoreBoilerplate.Api.Controllers;

/// <summary>
/// Manages announcements and notifications for users
/// </summary>
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

    /// <summary>
    /// Gets active announcements for the current user
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>List of active announcements</returns>
    /// <response code="200">Returns the list of announcements</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AnnouncementDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAnnouncements(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var announcements = await _announcementService.GetActiveAnnouncementsAsync(UserId, skip, take);
        return Ok(announcements);
    }

    /// <summary>
    /// Gets a specific announcement by ID
    /// </summary>
    /// <param name="id">The announcement ID</param>
    /// <returns>The announcement details</returns>
    /// <response code="200">Returns the announcement</response>
    /// <response code="404">If the announcement is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AnnouncementDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AnnouncementDto>> GetAnnouncement(Guid id)
    {
        var announcement = await _announcementService.GetByIdAsync(id);
        return Ok(announcement);
    }

    /// <summary>
    /// Creates a new announcement
    /// </summary>
    /// <param name="dto">The announcement data</param>
    /// <returns>The created announcement</returns>
    /// <response code="201">Returns the created announcement</response>
    /// <response code="400">If the data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AnnouncementDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<AnnouncementDto>> CreateAnnouncement([FromBody] CreateAnnouncementDto dto)
    {
        var announcement = await _announcementService.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetAnnouncement), new { id = announcement.Id }, announcement);
    }

    /// <summary>
    /// Updates an existing announcement
    /// </summary>
    /// <param name="id">The announcement ID</param>
    /// <param name="dto">The updated announcement data</param>
    /// <returns>The updated announcement</returns>
    /// <response code="200">Returns the updated announcement</response>
    /// <response code="400">If the data is invalid</response>
    /// <response code="404">If the announcement is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AnnouncementDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<AnnouncementDto>> UpdateAnnouncement(Guid id, [FromBody] UpdateAnnouncementDto dto)
    {
        var announcement = await _announcementService.UpdateAsync(UserId, id, dto);
        return Ok(announcement);
    }

    /// <summary>
    /// Deletes an announcement
    /// </summary>
    /// <param name="id">The announcement ID</param>
    /// <returns>No content</returns>
    /// <response code="204">If the announcement was deleted successfully</response>
    /// <response code="404">If the announcement is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> DeleteAnnouncement(Guid id)
    {
        await _announcementService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Gets user notifications
    /// </summary>
    /// <param name="isRead">Filter by read status</param>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>List of user notifications</returns>
    /// <response code="200">Returns the list of notifications</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(List<UserNotificationDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<UserNotificationDto>>> GetNotifications(
        [FromQuery] bool? isRead = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var notifications = await _announcementService.GetUserNotificationsAsync(UserId, isRead, skip, take);
        return Ok(notifications);
    }

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    /// <param name="id">The notification ID</param>
    /// <returns>The updated notification</returns>
    /// <response code="200">Returns the updated notification</response>
    /// <response code="404">If the notification is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPost("notifications/{id:guid}/read")]
    [ProducesResponseType(typeof(UserNotificationDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<UserNotificationDto>> MarkNotificationAsRead(Guid id)
    {
        var notification = await _announcementService.MarkNotificationAsReadAsync(UserId, id);
        return Ok(notification);
    }

    /// <summary>
    /// Gets user notification settings
    /// </summary>
    /// <returns>List of notification settings</returns>
    /// <response code="200">Returns the notification settings</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(List<NotificationSettingDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<NotificationSettingDto>>> GetNotificationSettings()
    {
        var settings = await _announcementService.GetUserNotificationSettingsAsync(UserId);
        return Ok(settings);
    }

    /// <summary>
    /// Updates user notification settings
    /// </summary>
    /// <param name="dto">The notification settings data</param>
    /// <returns>No content</returns>
    /// <response code="204">If the settings were updated successfully</response>
    /// <response code="400">If the data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPut("settings")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsDto dto)
    {
        await _announcementService.UpdateNotificationSettingsAsync(UserId, dto);
        return NoContent();
    }

    /// <summary>
    /// Registers a device for push notifications
    /// </summary>
    /// <param name="dto">The device registration data</param>
    /// <returns>The registered device</returns>
    /// <response code="200">Returns the registered device</response>
    /// <response code="400">If the data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPost("devices")]
    [ProducesResponseType(typeof(UserDeviceDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<UserDeviceDto>> RegisterDevice([FromBody] RegisterDeviceDto dto)
    {
        var device = await _announcementService.RegisterDeviceAsync(UserId, dto);
        return Ok(device);
    }
} 