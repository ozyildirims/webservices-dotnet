using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;
using HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Services;

public class ExamDefinitionService : IExamDefinitionService
{
    private readonly IExamDefinitionRepository _examRepository;

    public ExamDefinitionService(IExamDefinitionRepository examRepository)
    {
        _examRepository = examRepository;
    }

    public async Task<ExamDefinitionDto> GetExamByIdAsync(Guid id)
    {
        var exam = await _examRepository.GetByIdWithSectionsAsync(id);
        return exam == null ? null : MapToDto(exam);
    }

    public async Task<IEnumerable<ExamDefinitionListItemDto>> GetAllExamsAsync()
    {
        var exams = await _examRepository.GetAllAsync();
        return exams.Select(MapToListItemDto);
    }

    public async Task<IEnumerable<ExamDefinitionListItemDto>> GetActiveExamsAsync()
    {
        var exams = await _examRepository.GetActiveExamsAsync();
        return exams.Select(MapToListItemDto);
    }

    public async Task<IEnumerable<ExamDefinitionListItemDto>> GetExamsByTypeAsync(ExamType type)
    {
        var exams = await _examRepository.GetExamsByTypeAsync(type);
        return exams.Select(MapToListItemDto);
    }

    public async Task<ExamDefinitionDto> CreateExamAsync(CreateExamDefinitionDto dto, Guid createdById)
    {
        var exam = new ExamDefinition
        {
            Title = dto.Title,
            Description = dto.Description,
            Duration = dto.Duration,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Type = dto.Type,
            CreatedById = createdById,
            IsPublished = false,
            TotalPoints = dto.Sections?.Sum(s => s.Points) ?? 0,
            Results = new List<ExamResult>(),
            Sections = dto.Sections?.Select(s => new ExamSection
            {
                Title = s.Title,
                Description = s.Description,
                OrderIndex = s.OrderIndex,
                Points = s.Points,
                QuestionCount = s.QuestionCount,
                SuggestedDuration = s.SuggestedDuration
            }).ToList()
        };

        await _examRepository.AddAsync(exam);
        return MapToDto(exam);
    }

    public async Task<ExamDefinitionDto> UpdateExamAsync(Guid id, UpdateExamDefinitionDto dto)
    {
        var exam = await _examRepository.GetByIdWithSectionsAsync(id);
        if (exam == null) return null;

        exam.Title = dto.Title;
        exam.Description = dto.Description;
        exam.Duration = dto.Duration;
        exam.StartDate = dto.StartDate;
        exam.EndDate = dto.EndDate;

        if (dto.Sections != null)
        {
            foreach (var section in exam.Sections.ToList())
            {
                var updatedSection = dto.Sections.FirstOrDefault(s => s.Id == section.Id);
                if (updatedSection == null)
                {
                    section.IsDeleted = true;
                }
                else
                {
                    section.Title = updatedSection.Title;
                    section.Description = updatedSection.Description;
                    section.OrderIndex = updatedSection.OrderIndex;
                    section.Points = updatedSection.Points;
                    section.QuestionCount = updatedSection.QuestionCount;
                    section.SuggestedDuration = updatedSection.SuggestedDuration;
                }
            }

            var newSections = dto.Sections
                .Where(s => s.Id == Guid.Empty)
                .Select(s => new ExamSection
                {
                    Title = s.Title,
                    Description = s.Description,
                    OrderIndex = s.OrderIndex,
                    Points = s.Points,
                    QuestionCount = s.QuestionCount,
                    SuggestedDuration = s.SuggestedDuration,
                    ExamDefinitionId = exam.Id
                });

            foreach (var section in newSections)
            {
                exam.Sections.Add(section);
            }

            exam.TotalPoints = exam.Sections.Where(s => !s.IsDeleted).Sum(s => s.Points);
        }

        await _examRepository.UpdateAsync(exam);
        return MapToDto(exam);
    }

    public async Task DeleteExamAsync(Guid id)
    {
        await _examRepository.DeleteAsync(id);
    }

    public async Task<bool> PublishExamAsync(Guid id)
    {
        return await _examRepository.PublishExamAsync(id);
    }

    public async Task<bool> UnpublishExamAsync(Guid id)
    {
        return await _examRepository.UnpublishExamAsync(id);
    }

    private static ExamDefinitionDto MapToDto(ExamDefinition exam)
    {
        return new ExamDefinitionDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Duration = exam.Duration,
            TotalPoints = exam.TotalPoints,
            StartDate = exam.StartDate,
            EndDate = exam.EndDate,
            Type = exam.Type,
            IsPublished = exam.IsPublished,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt,
            Sections = exam.Sections?
                .Where(s => !s.IsDeleted)
                .Select(s => new ExamSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    OrderIndex = s.OrderIndex,
                    Points = s.Points,
                    QuestionCount = s.QuestionCount,
                    SuggestedDuration = s.SuggestedDuration,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToList()
        };
    }

    private static ExamDefinitionListItemDto MapToListItemDto(ExamDefinition exam)
    {
        return new ExamDefinitionListItemDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Type = exam.Type,
            StartDate = exam.StartDate,
            EndDate = exam.EndDate,
            IsPublished = exam.IsPublished,
            TotalPoints = exam.TotalPoints
        };
    }
} 