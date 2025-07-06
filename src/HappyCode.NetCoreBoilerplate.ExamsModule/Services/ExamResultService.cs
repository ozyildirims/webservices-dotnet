using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;
using HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Services;

public class ExamResultService : IExamResultService
{
    private readonly IExamResultRepository _resultRepository;
    private readonly IExamDefinitionRepository _examRepository;

    public ExamResultService(
        IExamResultRepository resultRepository,
        IExamDefinitionRepository examRepository)
    {
        _resultRepository = resultRepository;
        _examRepository = examRepository;
    }

    public async Task<ExamResultDto> GetResultByIdAsync(Guid id)
    {
        var result = await _resultRepository.GetByIdWithSectionResultsAsync(id);
        return result == null ? null : MapToDto(result);
    }

    public async Task<IEnumerable<ExamResultListItemDto>> GetStudentResultsAsync(Guid studentId)
    {
        var results = await _resultRepository.GetStudentResultsAsync(studentId);
        return results.Select(MapToListItemDto);
    }

    public async Task<IEnumerable<ExamResultListItemDto>> GetExamResultsAsync(Guid examId)
    {
        var results = await _resultRepository.GetExamResultsAsync(examId);
        return results.Select(MapToListItemDto);
    }

    public async Task<ExamResultDto> GetStudentExamResultAsync(Guid studentId, Guid examId)
    {
        var result = await _resultRepository.GetStudentExamResultAsync(studentId, examId);
        return result == null ? null : MapToDto(result);
    }

    public async Task<ExamResultDto> StartExamAsync(CreateExamResultDto dto)
    {
        var exam = await _examRepository.GetByIdWithSectionsAsync(dto.ExamDefinitionId);
        if (exam == null) throw new InvalidOperationException("Exam not found");
        if (!exam.IsPublished) throw new InvalidOperationException("Exam is not published");
        
        var hasCompleted = await _resultRepository.HasStudentCompletedExamAsync(dto.StudentId, dto.ExamDefinitionId);
        if (hasCompleted) throw new InvalidOperationException("Student has already completed this exam");

        var result = new ExamResult
        {
            StudentId = dto.StudentId,
            ExamDefinitionId = dto.ExamDefinitionId,
            StartTime = dto.StartTime,
            Status = ExamStatus.InProgress,
            SectionResults = dto.SectionResults.Select(s => new SectionResult
            {
                ExamSectionId = s.ExamSectionId,
                Score = s.Score,
                TimeTaken = s.TimeTaken,
                Percentage = CalculatePercentage(s.Score, exam.Sections.First(es => es.Id == s.ExamSectionId).Points)
            }).ToList()
        };

        result.TotalScore = result.SectionResults.Sum(s => s.Score);
        result.Percentage = CalculatePercentage(result.TotalScore, exam.TotalPoints);

        await _resultRepository.AddAsync(result);
        return MapToDto(result);
    }

    public async Task<ExamResultDto> UpdateExamResultAsync(Guid id, UpdateExamResultDto dto)
    {
        var result = await _resultRepository.GetByIdWithSectionResultsAsync(id);
        if (result == null) return null;

        var exam = await _examRepository.GetByIdWithSectionsAsync(result.ExamDefinitionId);
        if (exam == null) throw new InvalidOperationException("Exam not found");

        result.EndTime = dto.EndTime;
        result.Status = dto.Status;

        if (dto.SectionResults != null)
        {
            foreach (var sectionResult in result.SectionResults)
            {
                var updatedSection = dto.SectionResults.FirstOrDefault(s => s.ExamSectionId == sectionResult.ExamSectionId);
                if (updatedSection != null)
                {
                    sectionResult.Score = updatedSection.Score;
                    sectionResult.TimeTaken = updatedSection.TimeTaken;
                    sectionResult.Percentage = CalculatePercentage(
                        updatedSection.Score,
                        exam.Sections.First(s => s.Id == updatedSection.ExamSectionId).Points
                    );
                }
            }

            result.TotalScore = result.SectionResults.Sum(s => s.Score);
            result.Percentage = CalculatePercentage(result.TotalScore, exam.TotalPoints);
        }

        await _resultRepository.UpdateAsync(result);
        return MapToDto(result);
    }

    public async Task<bool> HasStudentCompletedExamAsync(Guid studentId, Guid examId)
    {
        return await _resultRepository.HasStudentCompletedExamAsync(studentId, examId);
    }

    public async Task<byte[]> ExportExamResultsToExcelAsync(Guid examId)
    {
        var exam = await _examRepository.GetByIdWithSectionsAsync(examId);
        if (exam == null) throw new InvalidOperationException("Exam not found");

        var results = await _resultRepository.GetExamResultsAsync(examId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Results");

        // Headers
        worksheet.Cell(1, 1).Value = "Student ID";
        worksheet.Cell(1, 2).Value = "Start Time";
        worksheet.Cell(1, 3).Value = "End Time";
        worksheet.Cell(1, 4).Value = "Total Score";
        worksheet.Cell(1, 5).Value = "Percentage";
        worksheet.Cell(1, 6).Value = "Status";

        var row = 2;
        foreach (var result in results)
        {
            worksheet.Cell(row, 1).Value = result.StudentId.ToString();
            worksheet.Cell(row, 2).Value = result.StartTime;
            worksheet.Cell(row, 3).Value = result.EndTime;
            worksheet.Cell(row, 4).Value = result.TotalScore;
            worksheet.Cell(row, 5).Value = result.Percentage;
            worksheet.Cell(row, 6).Value = result.Status.ToString();
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<bool> ImportExamResultsFromExcelAsync(Guid examId, byte[] excelData)
    {
        var exam = await _examRepository.GetByIdWithSectionsAsync(examId);
        if (exam == null) throw new InvalidOperationException("Exam not found");

        using var stream = new MemoryStream(excelData);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        var row = 2;
        while (!worksheet.Cell(row, 1).IsEmpty())
        {
            var studentId = Guid.Parse(worksheet.Cell(row, 1).GetString());
            var result = await _resultRepository.GetStudentExamResultAsync(studentId, examId);

            if (result != null)
            {
                result.TotalScore = worksheet.Cell(row, 4).GetValue<int>();
                result.Percentage = worksheet.Cell(row, 5).GetValue<decimal>();
                result.Status = Enum.Parse<ExamStatus>(worksheet.Cell(row, 6).GetString());
                await _resultRepository.UpdateAsync(result);
            }

            row++;
        }

        return true;
    }

    private static decimal CalculatePercentage(int score, int total)
    {
        return total == 0 ? 0 : Math.Round((decimal)score / total * 100, 2);
    }

    private static ExamResultDto MapToDto(ExamResult result)
    {
        return new ExamResultDto
        {
            Id = result.Id,
            StudentId = result.StudentId,
            ExamDefinitionId = result.ExamDefinitionId,
            StartTime = result.StartTime,
            EndTime = result.EndTime,
            TotalScore = result.TotalScore,
            Percentage = result.Percentage,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt,
            SectionResults = result.SectionResults?
                .Where(s => !s.IsDeleted)
                .Select(s => new SectionResultDto
                {
                    Id = s.Id,
                    ExamSectionId = s.ExamSectionId,
                    Score = s.Score,
                    Percentage = s.Percentage,
                    TimeTaken = s.TimeTaken,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToList()
        };
    }

    private static ExamResultListItemDto MapToListItemDto(ExamResult result)
    {
        return new ExamResultListItemDto
        {
            Id = result.Id,
            ExamTitle = result.ExamDefinition?.Title ?? "Unknown Exam",
            StartTime = result.StartTime,
            EndTime = result.EndTime,
            TotalScore = result.TotalScore,
            Percentage = result.Percentage,
            Status = result.Status
        };
    }
} 