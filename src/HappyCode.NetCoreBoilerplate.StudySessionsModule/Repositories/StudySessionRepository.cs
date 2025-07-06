using HappyCode.NetCoreBoilerplate.StudySessionsModule.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Repositories;

public interface IStudySessionRepository
{
    Task<List<StudySession>> GetActiveSessionsAsync(DateTime? fromDate = null, DateTime? toDate = null, Guid? teacherId = null, int skip = 0, int take = 10);
    Task<StudySession?> GetByIdAsync(Guid id);
    Task<StudySession> CreateAsync(StudySession session);
    Task<StudySession> UpdateAsync(StudySession session);
    Task DeleteAsync(Guid id);
    Task<List<StudySessionReservation>> GetUserReservationsAsync(Guid userId, string? status = null, int skip = 0, int take = 10);
    Task<StudySessionReservation?> GetReservationByIdAsync(Guid id);
    Task<StudySessionReservation> CreateReservationAsync(StudySessionReservation reservation);
    Task<StudySessionReservation> UpdateReservationAsync(StudySessionReservation reservation);
    Task<List<StudySessionWaitlist>> GetSessionWaitlistAsync(Guid sessionId);
    Task<StudySessionWaitlist?> GetWaitlistEntryByIdAsync(Guid id);
    Task<StudySessionWaitlist> CreateWaitlistEntryAsync(StudySessionWaitlist entry);
    Task<StudySessionWaitlist> UpdateWaitlistEntryAsync(StudySessionWaitlist entry);
    Task<int> GetNextWaitlistPositionAsync(Guid sessionId);
    Task<bool> HasActiveReservationAsync(Guid sessionId, Guid studentId);
    Task<bool> IsOnWaitlistAsync(Guid sessionId, Guid studentId);
}

public class StudySessionRepository : IStudySessionRepository
{
    private readonly StudySessionsContext _context;

    public StudySessionRepository(StudySessionsContext context)
    {
        _context = context;
    }

    public async Task<List<StudySession>> GetActiveSessionsAsync(DateTime? fromDate = null, DateTime? toDate = null, Guid? teacherId = null, int skip = 0, int take = 10)
    {
        var query = _context.StudySessions
            .Include(s => s.Teacher)
            .Include(s => s.RecurrenceRule)
            .Include(s => s.Reservations)
                .ThenInclude(r => r.Student)
            .Where(s => s.IsActive && !s.IsCancelled);

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.StartTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.StartTime <= toDate.Value);
        }

        if (teacherId.HasValue)
        {
            query = query.Where(s => s.TeacherId == teacherId.Value);
        }

        return await query
            .OrderBy(s => s.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<StudySession?> GetByIdAsync(Guid id)
    {
        return await _context.StudySessions
            .Include(s => s.Teacher)
            .Include(s => s.RecurrenceRule)
            .Include(s => s.Reservations)
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StudySession> CreateAsync(StudySession session)
    {
        _context.StudySessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<StudySession> UpdateAsync(StudySession session)
    {
        _context.Entry(session).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task DeleteAsync(Guid id)
    {
        var session = await _context.StudySessions.FindAsync(id);
        if (session != null)
        {
            _context.StudySessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<StudySessionReservation>> GetUserReservationsAsync(Guid userId, string? status = null, int skip = 0, int take = 10)
    {
        var query = _context.StudySessionReservations
            .Include(r => r.Session)
                .ThenInclude(s => s.Teacher)
            .Where(r => r.StudentId == userId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<StudySessionReservation?> GetReservationByIdAsync(Guid id)
    {
        return await _context.StudySessionReservations
            .Include(r => r.Session)
                .ThenInclude(s => s.Teacher)
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<StudySessionReservation> CreateReservationAsync(StudySessionReservation reservation)
    {
        _context.StudySessionReservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<StudySessionReservation> UpdateReservationAsync(StudySessionReservation reservation)
    {
        _context.Entry(reservation).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<List<StudySessionWaitlist>> GetSessionWaitlistAsync(Guid sessionId)
    {
        return await _context.StudySessionWaitlist
            .Include(w => w.Student)
            .Where(w => w.SessionId == sessionId && w.Status == WaitlistStatus.Active.ToString())
            .OrderBy(w => w.Position)
            .ToListAsync();
    }

    public async Task<StudySessionWaitlist?> GetWaitlistEntryByIdAsync(Guid id)
    {
        return await _context.StudySessionWaitlist
            .Include(w => w.Session)
            .Include(w => w.Student)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<StudySessionWaitlist> CreateWaitlistEntryAsync(StudySessionWaitlist entry)
    {
        _context.StudySessionWaitlist.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<StudySessionWaitlist> UpdateWaitlistEntryAsync(StudySessionWaitlist entry)
    {
        _context.Entry(entry).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<int> GetNextWaitlistPositionAsync(Guid sessionId)
    {
        var maxPosition = await _context.StudySessionWaitlist
            .Where(w => w.SessionId == sessionId)
            .MaxAsync(w => (int?)w.Position) ?? 0;

        return maxPosition + 1;
    }

    public async Task<bool> HasActiveReservationAsync(Guid sessionId, Guid studentId)
    {
        return await _context.StudySessionReservations
            .AnyAsync(r => r.SessionId == sessionId && 
                          r.StudentId == studentId && 
                          (r.Status == ReservationStatus.Pending.ToString() || 
                           r.Status == ReservationStatus.Confirmed.ToString()));
    }

    public async Task<bool> IsOnWaitlistAsync(Guid sessionId, Guid studentId)
    {
        return await _context.StudySessionWaitlist
            .AnyAsync(w => w.SessionId == sessionId && 
                          w.StudentId == studentId && 
                          w.Status == WaitlistStatus.Active.ToString());
    }
} 