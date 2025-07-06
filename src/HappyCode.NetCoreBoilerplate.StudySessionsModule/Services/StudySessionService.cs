using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Services;
using HappyCode.NetCoreBoilerplate.StudySessionsModule.Dtos;
using HappyCode.NetCoreBoilerplate.StudySessionsModule.Models;
using HappyCode.NetCoreBoilerplate.StudySessionsModule.Repositories;
using Microsoft.Extensions.Logging;

namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Services;

public interface IStudySessionService
{
    Task<List<StudySessionDto>> GetActiveSessionsAsync(DateTime? fromDate = null, DateTime? toDate = null, Guid? teacherId = null, int skip = 0, int take = 10);
    Task<StudySessionDto> GetByIdAsync(Guid id);
    Task<StudySessionDto> CreateAsync(Guid createdBy, CreateStudySessionDto dto);
    Task<StudySessionDto> UpdateAsync(Guid modifiedBy, Guid id, UpdateStudySessionDto dto);
    Task<StudySessionDto> CancelAsync(Guid modifiedBy, Guid id, CancelStudySessionDto dto);
    Task DeleteAsync(Guid id);
    Task<List<StudySessionReservationDto>> GetUserReservationsAsync(Guid userId, string? status = null, int skip = 0, int take = 10);
    Task<StudySessionReservationDto> CreateReservationAsync(Guid studentId, CreateStudySessionReservationDto dto);
    Task<StudySessionReservationDto> CancelReservationAsync(Guid studentId, Guid reservationId, CancelStudySessionReservationDto dto);
    Task<List<StudySessionWaitlistDto>> GetSessionWaitlistAsync(Guid sessionId);
    Task<StudySessionWaitlistDto> JoinWaitlistAsync(Guid studentId, JoinWaitlistDto dto);
    Task<StudySessionWaitlistDto> RespondToWaitlistOfferAsync(Guid studentId, Guid waitlistId, WaitlistOfferResponseDto dto);
}

public class StudySessionService : IStudySessionService
{
    private readonly IStudySessionRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StudySessionService> _logger;

    public StudySessionService(
        IStudySessionRepository repository,
        INotificationService notificationService,
        ILogger<StudySessionService> logger)
    {
        _repository = repository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<List<StudySessionDto>> GetActiveSessionsAsync(DateTime? fromDate = null, DateTime? toDate = null, Guid? teacherId = null, int skip = 0, int take = 10)
    {
        var sessions = await _repository.GetActiveSessionsAsync(fromDate, toDate, teacherId, skip, take);
        return sessions.Select(ToDto).ToList();
    }

    public async Task<StudySessionDto> GetByIdAsync(Guid id)
    {
        var session = await _repository.GetByIdAsync(id);
        if (session == null)
        {
            throw new InvalidOperationException($"Study session with ID {id} not found");
        }
        return ToDto(session);
    }

    public async Task<StudySessionDto> CreateAsync(Guid createdBy, CreateStudySessionDto dto)
    {
        var session = new StudySession
        {
            Title = dto.Title,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            MaxCapacity = dto.MaxCapacity,
            Location = dto.Location,
            TeacherId = dto.TeacherId,
            SubjectId = dto.SubjectId,
            IsActive = true,
            IsCancelled = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.RecurrenceRule != null)
        {
            session.RecurrenceRule = new StudySessionRecurrenceRule
            {
                Pattern = dto.RecurrenceRule.Pattern,
                DaysOfWeek = dto.RecurrenceRule.DaysOfWeek,
                StartDate = dto.RecurrenceRule.StartDate,
                EndDate = dto.RecurrenceRule.EndDate,
                CreatedAt = DateTime.UtcNow
            };
        }

        session = await _repository.CreateAsync(session);

        // Notify teacher about the new session
        await _notificationService.SendCustomNotificationAsync(
            "New Study Session Created",
            $"You have been assigned to teach {session.Title} starting at {session.StartTime:g}",
            new List<Guid> { session.TeacherId });

        return ToDto(session);
    }

    public async Task<StudySessionDto> UpdateAsync(Guid modifiedBy, Guid id, UpdateStudySessionDto dto)
    {
        var session = await _repository.GetByIdAsync(id);
        if (session == null)
        {
            throw new InvalidOperationException($"Study session with ID {id} not found");
        }

        session.Title = dto.Title;
        session.Description = dto.Description;
        session.StartTime = dto.StartTime;
        session.EndTime = dto.EndTime;
        session.MaxCapacity = dto.MaxCapacity;
        session.Location = dto.Location;
        session.TeacherId = dto.TeacherId;
        session.SubjectId = dto.SubjectId;
        session.IsActive = dto.IsActive;
        session.ModifiedBy = modifiedBy;
        session.ModifiedAt = DateTime.UtcNow;

        if (dto.RecurrenceRule != null)
        {
            if (session.RecurrenceRule == null)
            {
                session.RecurrenceRule = new StudySessionRecurrenceRule();
            }

            session.RecurrenceRule.Pattern = dto.RecurrenceRule.Pattern;
            session.RecurrenceRule.DaysOfWeek = dto.RecurrenceRule.DaysOfWeek;
            session.RecurrenceRule.StartDate = dto.RecurrenceRule.StartDate;
            session.RecurrenceRule.EndDate = dto.RecurrenceRule.EndDate;
            session.RecurrenceRule.ModifiedAt = DateTime.UtcNow;
        }

        session = await _repository.UpdateAsync(session);

        // Notify participants about the changes
        var studentIds = session.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed.ToString())
            .Select(r => r.StudentId)
            .ToList();

        if (studentIds.Any())
        {
            await _notificationService.SendCustomNotificationAsync(
                "Study Session Updated",
                $"The study session {session.Title} has been updated. Please check the new details.",
                studentIds);
        }

        return ToDto(session);
    }

    public async Task<StudySessionDto> CancelAsync(Guid modifiedBy, Guid id, CancelStudySessionDto dto)
    {
        var session = await _repository.GetByIdAsync(id);
        if (session == null)
        {
            throw new InvalidOperationException($"Study session with ID {id} not found");
        }

        session.IsCancelled = true;
        session.CancellationReason = dto.CancellationReason;
        session.ModifiedBy = modifiedBy;
        session.ModifiedAt = DateTime.UtcNow;

        session = await _repository.UpdateAsync(session);

        // Notify participants about cancellation
        var studentIds = session.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed.ToString())
            .Select(r => r.StudentId)
            .ToList();

        if (studentIds.Any())
        {
            await _notificationService.SendCustomNotificationAsync(
                "Study Session Cancelled",
                $"The study session {session.Title} has been cancelled. Reason: {dto.CancellationReason}",
                studentIds);
        }

        return ToDto(session);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<List<StudySessionReservationDto>> GetUserReservationsAsync(Guid userId, string? status = null, int skip = 0, int take = 10)
    {
        var reservations = await _repository.GetUserReservationsAsync(userId, status, skip, take);
        return reservations.Select(ToDto).ToList();
    }

    public async Task<StudySessionReservationDto> CreateReservationAsync(Guid studentId, CreateStudySessionReservationDto dto)
    {
        var session = await _repository.GetByIdAsync(dto.SessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Study session with ID {dto.SessionId} not found");
        }

        if (session.IsCancelled)
        {
            throw new InvalidOperationException("Cannot reserve a cancelled session");
        }

        if (session.CurrentCapacity >= session.MaxCapacity)
        {
            throw new InvalidOperationException("Session is at full capacity");
        }

        if (await _repository.HasActiveReservationAsync(dto.SessionId, studentId))
        {
            throw new InvalidOperationException("Student already has an active reservation for this session");
        }

        var reservation = new StudySessionReservation
        {
            SessionId = dto.SessionId,
            StudentId = studentId,
            Status = ReservationStatus.Confirmed.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        reservation = await _repository.CreateReservationAsync(reservation);

        // Update session capacity
        session.CurrentCapacity++;
        await _repository.UpdateAsync(session);

        // Notify teacher about the new reservation
        await _notificationService.SendCustomNotificationAsync(
            "New Session Reservation",
            $"A new student has reserved a spot in your session {session.Title}",
            new List<Guid> { session.TeacherId });

        return ToDto(reservation);
    }

    public async Task<StudySessionReservationDto> CancelReservationAsync(Guid studentId, Guid reservationId, CancelStudySessionReservationDto dto)
    {
        var reservation = await _repository.GetReservationByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new InvalidOperationException($"Reservation with ID {reservationId} not found");
        }

        if (reservation.StudentId != studentId)
        {
            throw new InvalidOperationException("Cannot cancel another student's reservation");
        }

        if (reservation.Status == ReservationStatus.Cancelled.ToString())
        {
            throw new InvalidOperationException("Reservation is already cancelled");
        }

        reservation.Status = ReservationStatus.Cancelled.ToString();
        reservation.CancellationReason = dto.CancellationReason;
        reservation.ModifiedAt = DateTime.UtcNow;

        reservation = await _repository.UpdateReservationAsync(reservation);

        // Update session capacity
        var session = await _repository.GetByIdAsync(reservation.SessionId);
        if (session != null)
        {
            session.CurrentCapacity--;
            await _repository.UpdateAsync(session);

            // Notify teacher about the cancellation
            await _notificationService.SendCustomNotificationAsync(
                "Session Reservation Cancelled",
                $"A student has cancelled their reservation for {session.Title}. Reason: {dto.CancellationReason}",
                new List<Guid> { session.TeacherId });

            // Process waitlist if there are any entries
            await ProcessWaitlistAsync(session);
        }

        return ToDto(reservation);
    }

    public async Task<List<StudySessionWaitlistDto>> GetSessionWaitlistAsync(Guid sessionId)
    {
        var waitlist = await _repository.GetSessionWaitlistAsync(sessionId);
        return waitlist.Select(ToDto).ToList();
    }

    public async Task<StudySessionWaitlistDto> JoinWaitlistAsync(Guid studentId, JoinWaitlistDto dto)
    {
        var session = await _repository.GetByIdAsync(dto.SessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Study session with ID {dto.SessionId} not found");
        }

        if (session.IsCancelled)
        {
            throw new InvalidOperationException("Cannot join waitlist for a cancelled session");
        }

        if (await _repository.HasActiveReservationAsync(dto.SessionId, studentId))
        {
            throw new InvalidOperationException("Student already has an active reservation for this session");
        }

        if (await _repository.IsOnWaitlistAsync(dto.SessionId, studentId))
        {
            throw new InvalidOperationException("Student is already on the waitlist for this session");
        }

        var position = await _repository.GetNextWaitlistPositionAsync(dto.SessionId);
        var entry = new StudySessionWaitlist
        {
            SessionId = dto.SessionId,
            StudentId = studentId,
            Position = position,
            Status = WaitlistStatus.Active.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        entry = await _repository.CreateWaitlistEntryAsync(entry);
        return ToDto(entry);
    }

    public async Task<StudySessionWaitlistDto> RespondToWaitlistOfferAsync(Guid studentId, Guid waitlistId, WaitlistOfferResponseDto dto)
    {
        var entry = await _repository.GetWaitlistEntryByIdAsync(waitlistId);
        if (entry == null)
        {
            throw new InvalidOperationException($"Waitlist entry with ID {waitlistId} not found");
        }

        if (entry.StudentId != studentId)
        {
            throw new InvalidOperationException("Cannot respond to another student's waitlist offer");
        }

        if (entry.Status != WaitlistStatus.Offered.ToString())
        {
            throw new InvalidOperationException("No active offer for this waitlist entry");
        }

        entry.Status = dto.Accept ? WaitlistStatus.Accepted.ToString() : WaitlistStatus.Declined.ToString();
        entry.ModifiedAt = DateTime.UtcNow;

        entry = await _repository.UpdateWaitlistEntryAsync(entry);

        if (dto.Accept)
        {
            // Create reservation
            var reservation = new StudySessionReservation
            {
                SessionId = entry.SessionId,
                StudentId = studentId,
                Status = ReservationStatus.Confirmed.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateReservationAsync(reservation);

            // Update session capacity
            var session = await _repository.GetByIdAsync(entry.SessionId);
            if (session != null)
            {
                session.CurrentCapacity++;
                await _repository.UpdateAsync(session);

                // Notify teacher about the new reservation
                await _notificationService.SendCustomNotificationAsync(
                    "Waitlist Student Accepted",
                    $"A student from the waitlist has accepted a spot in your session {session.Title}",
                    new List<Guid> { session.TeacherId });
            }
        }

        return ToDto(entry);
    }

    private async Task ProcessWaitlistAsync(StudySession session)
    {
        if (session.CurrentCapacity >= session.MaxCapacity)
        {
            return;
        }

        var waitlist = await _repository.GetSessionWaitlistAsync(session.Id);
        var nextEntry = waitlist.FirstOrDefault();

        if (nextEntry != null)
        {
            nextEntry.Status = WaitlistStatus.Offered.ToString();
            nextEntry.ModifiedAt = DateTime.UtcNow;
            await _repository.UpdateWaitlistEntryAsync(nextEntry);

            // Notify student about the offer
            await _notificationService.SendCustomNotificationAsync(
                "Study Session Spot Available",
                $"A spot has opened up in {session.Title}. You have 24 hours to accept this offer.",
                new List<Guid> { nextEntry.StudentId });
        }
    }

    private static StudySessionDto ToDto(StudySession session) => new()
    {
        Id = session.Id,
        Title = session.Title,
        Description = session.Description,
        StartTime = session.StartTime,
        EndTime = session.EndTime,
        MaxCapacity = session.MaxCapacity,
        CurrentCapacity = session.CurrentCapacity,
        Location = session.Location,
        TeacherId = session.TeacherId,
        TeacherName = session.Teacher?.Name ?? "Unknown",
        SubjectId = session.SubjectId,
        IsActive = session.IsActive,
        IsCancelled = session.IsCancelled,
        CancellationReason = session.CancellationReason,
        CreatedAt = session.CreatedAt,
        RecurrenceRule = session.RecurrenceRule != null ? new StudySessionRecurrenceRuleDto
        {
            Id = session.RecurrenceRule.Id,
            Pattern = session.RecurrenceRule.Pattern,
            DaysOfWeek = session.RecurrenceRule.DaysOfWeek,
            StartDate = session.RecurrenceRule.StartDate,
            EndDate = session.RecurrenceRule.EndDate
        } : null,
        Reservations = session.Reservations.Select(ToDto).ToList()
    };

    private static StudySessionReservationDto ToDto(StudySessionReservation reservation) => new()
    {
        Id = reservation.Id,
        SessionId = reservation.SessionId,
        StudentId = reservation.StudentId,
        StudentName = reservation.Student?.Name ?? "Unknown",
        Status = reservation.Status,
        CancellationReason = reservation.CancellationReason,
        CreatedAt = reservation.CreatedAt,
        ModifiedAt = reservation.ModifiedAt
    };

    private static StudySessionWaitlistDto ToDto(StudySessionWaitlist entry) => new()
    {
        Id = entry.Id,
        SessionId = entry.SessionId,
        StudentId = entry.StudentId,
        StudentName = entry.Student?.Name ?? "Unknown",
        Position = entry.Position,
        Status = entry.Status,
        CreatedAt = entry.CreatedAt,
        ModifiedAt = entry.ModifiedAt
    };
} 