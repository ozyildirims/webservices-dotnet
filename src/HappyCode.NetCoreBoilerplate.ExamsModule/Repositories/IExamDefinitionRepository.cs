using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;

public interface IExamDefinitionRepository : IBaseRepository<ExamDefinition>
{
    Task<ExamDefinition> GetByIdWithSectionsAsync(Guid id);
    Task<IEnumerable<ExamDefinition>> GetActiveExamsAsync();
    Task<IEnumerable<ExamDefinition>> GetExamsByTypeAsync(ExamType type);
    Task<bool> PublishExamAsync(Guid id);
    Task<bool> UnpublishExamAsync(Guid id);
} 