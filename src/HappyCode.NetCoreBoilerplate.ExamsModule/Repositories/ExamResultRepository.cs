using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;

public class ExamResultRepository : BaseRepository<ExamResult>, IExamResultRepository
{
    public ExamResultRepository(DbContext context) : base(context)
    {
    }

    public async Task<ExamResult> GetByIdWithSectionResultsAsync(Guid id)
    {
        return await DbSet
            .Include(r => r.SectionResults.Where(sr => !sr.IsDeleted))
            .Include(r => r.ExamDefinition)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<IEnumerable<ExamResult>> GetStudentResultsAsync(Guid studentId)
    {
        return await DbSet
            .Include(r => r.ExamDefinition)
            .Where(r => !r.IsDeleted && r.StudentId == studentId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExamResult>> GetExamResultsAsync(Guid examId)
    {
        return await DbSet
            .Include(r => r.SectionResults.Where(sr => !sr.IsDeleted))
            .Where(r => !r.IsDeleted && r.ExamDefinitionId == examId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<ExamResult> GetStudentExamResultAsync(Guid studentId, Guid examId)
    {
        return await DbSet
            .Include(r => r.SectionResults.Where(sr => !sr.IsDeleted))
            .FirstOrDefaultAsync(r => !r.IsDeleted && r.StudentId == studentId && r.ExamDefinitionId == examId);
    }

    public async Task<bool> HasStudentCompletedExamAsync(Guid studentId, Guid examId)
    {
        return await DbSet
            .AnyAsync(r => !r.IsDeleted && r.StudentId == studentId && r.ExamDefinitionId == examId && r.Status == ExamStatus.Completed);
    }
} 