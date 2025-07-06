using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;

public interface IExamResultRepository : IBaseRepository<ExamResult>
{
    Task<ExamResult> GetByIdWithSectionResultsAsync(Guid id);
    Task<IEnumerable<ExamResult>> GetStudentResultsAsync(Guid studentId);
    Task<IEnumerable<ExamResult>> GetExamResultsAsync(Guid examId);
    Task<ExamResult> GetStudentExamResultAsync(Guid studentId, Guid examId);
    Task<bool> HasStudentCompletedExamAsync(Guid studentId, Guid examId);
} 