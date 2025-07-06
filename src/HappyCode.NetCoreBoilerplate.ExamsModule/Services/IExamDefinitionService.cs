using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Services;

public interface IExamDefinitionService
{
    Task<ExamDefinitionDto> GetExamByIdAsync(Guid id);
    Task<IEnumerable<ExamDefinitionListItemDto>> GetAllExamsAsync();
    Task<IEnumerable<ExamDefinitionListItemDto>> GetActiveExamsAsync();
    Task<IEnumerable<ExamDefinitionListItemDto>> GetExamsByTypeAsync(ExamType type);
    Task<ExamDefinitionDto> CreateExamAsync(CreateExamDefinitionDto dto, Guid createdById);
    Task<ExamDefinitionDto> UpdateExamAsync(Guid id, UpdateExamDefinitionDto dto);
    Task DeleteExamAsync(Guid id);
    Task<bool> PublishExamAsync(Guid id);
    Task<bool> UnpublishExamAsync(Guid id);
} 