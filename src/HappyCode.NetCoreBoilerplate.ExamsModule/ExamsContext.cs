using Microsoft.EntityFrameworkCore;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;

namespace HappyCode.NetCoreBoilerplate.ExamsModule;

public class ExamsContext : DbContext
{
    public ExamsContext(DbContextOptions<ExamsContext> options) : base(options)
    {
    }

    public DbSet<ExamDefinition> ExamDefinitions { get; set; }
    public DbSet<ExamSection> ExamSections { get; set; }
    public DbSet<ExamResult> ExamResults { get; set; }
    public DbSet<SectionResult> SectionResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExamDefinition>()
            .HasMany(e => e.Sections)
            .WithOne(s => s.ExamDefinition)
            .HasForeignKey(s => s.ExamDefinitionId);

        modelBuilder.Entity<ExamDefinition>()
            .HasMany(e => e.Results)
            .WithOne(r => r.ExamDefinition)
            .HasForeignKey(r => r.ExamDefinitionId);

        modelBuilder.Entity<ExamResult>()
            .HasMany(r => r.SectionResults)
            .WithOne(sr => sr.ExamResult)
            .HasForeignKey(sr => sr.ExamResultId);

        modelBuilder.Entity<SectionResult>()
            .HasOne(sr => sr.ExamSection)
            .WithMany()
            .HasForeignKey(sr => sr.ExamSectionId);
    }
} 