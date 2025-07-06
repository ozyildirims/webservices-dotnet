using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;

public class ExamDefinitionRepository : BaseRepository<ExamDefinition>, IExamDefinitionRepository
{
    public ExamDefinitionRepository(DbContext context) : base(context)
    {
    }

    public async Task<ExamDefinition> GetByIdWithSectionsAsync(Guid id)
    {
        return await DbSet
            .Include(e => e.Sections.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IEnumerable<ExamDefinition>> GetActiveExamsAsync()
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(e => !e.IsDeleted && e.IsPublished && e.StartDate <= now && e.EndDate >= now)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExamDefinition>> GetExamsByTypeAsync(ExamType type)
    {
        return await DbSet
            .Where(e => !e.IsDeleted && e.Type == type)
            .ToListAsync();
    }

    public async Task<bool> PublishExamAsync(Guid id)
    {
        var exam = await GetByIdAsync(id);
        if (exam == null) return false;

        exam.IsPublished = true;
        exam.UpdatedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpublishExamAsync(Guid id)
    {
        var exam = await GetByIdAsync(id);
        if (exam == null) return false;

        exam.IsPublished = false;
        exam.UpdatedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        return true;
    }
} 