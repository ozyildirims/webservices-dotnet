using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Services;

public interface IExamResultService
{
    Task<ExamResultDto> GetResultByIdAsync(Guid id);
    Task<IEnumerable<ExamResultListItemDto>> GetStudentResultsAsync(Guid studentId);
    Task<IEnumerable<ExamResultListItemDto>> GetExamResultsAsync(Guid examId);
    Task<ExamResultDto> GetStudentExamResultAsync(Guid studentId, Guid examId);
    Task<ExamResultDto> StartExamAsync(CreateExamResultDto dto);
    Task<ExamResultDto> UpdateExamResultAsync(Guid id, UpdateExamResultDto dto);
    Task<bool> HasStudentCompletedExamAsync(Guid studentId, Guid examId);
    Task<byte[]> ExportExamResultsToExcelAsync(Guid examId);
    Task<bool> ImportExamResultsFromExcelAsync(Guid examId, byte[] excelData);
} 