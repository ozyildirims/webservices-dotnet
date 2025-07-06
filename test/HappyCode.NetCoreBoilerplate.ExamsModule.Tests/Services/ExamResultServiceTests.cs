using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;
using HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;
using HappyCode.NetCoreBoilerplate.ExamsModule.Services;
using Moq;
using Xunit;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Tests.Services;

public class ExamResultServiceTests
{
    private readonly Mock<IExamResultRepository> _mockResultRepository;
    private readonly Mock<IExamDefinitionRepository> _mockExamRepository;
    private readonly ExamResultService _service;

    public ExamResultServiceTests()
    {
        _mockResultRepository = new Mock<IExamResultRepository>();
        _mockExamRepository = new Mock<IExamDefinitionRepository>();
        _service = new ExamResultService(_mockResultRepository.Object, _mockExamRepository.Object);
    }

    [Fact]
    public async Task GetResultByIdAsync_ExistingResult_ReturnsResultDto()
    {
        // Arrange
        var resultId = Guid.NewGuid();
        var result = new ExamResult
        {
            Id = resultId,
            StudentId = Guid.NewGuid(),
            ExamDefinitionId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            TotalScore = 85,
            Percentage = 85,
            Status = ExamStatus.Completed,
            SectionResults = new List<SectionResult>
            {
                new SectionResult
                {
                    Id = Guid.NewGuid(),
                    ExamSectionId = Guid.NewGuid(),
                    Score = 85,
                    Percentage = 85,
                    TimeTaken = TimeSpan.FromMinutes(30)
                }
            }
        };

        _mockResultRepository.Setup(r => r.GetByIdWithSectionResultsAsync(resultId))
            .ReturnsAsync(result);

        // Act
        var dto = await _service.GetResultByIdAsync(resultId);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(resultId, dto.Id);
        Assert.Equal(result.StudentId, dto.StudentId);
        Assert.Equal(result.TotalScore, dto.TotalScore);
        Assert.Equal(result.Status, dto.Status);
        Assert.Single(dto.SectionResults);
    }

    [Fact]
    public async Task StartExamAsync_ValidDto_CreatesAndReturnsResult()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var exam = new ExamDefinition
        {
            Id = examId,
            IsPublished = true,
            TotalPoints = 100,
            Sections = new List<ExamSection>
            {
                new ExamSection
                {
                    Id = Guid.NewGuid(),
                    Points = 100
                }
            }
        };

        var dto = new CreateExamResultDto
        {
            StudentId = Guid.NewGuid(),
            ExamDefinitionId = examId,
            StartTime = DateTime.UtcNow,
            SectionResults = new List<CreateSectionResultDto>
            {
                new CreateSectionResultDto
                {
                    ExamSectionId = exam.Sections.First().Id,
                    Score = 85,
                    TimeTaken = TimeSpan.FromMinutes(30)
                }
            }
        };

        _mockExamRepository.Setup(r => r.GetByIdWithSectionsAsync(examId))
            .ReturnsAsync(exam);
        _mockResultRepository.Setup(r => r.HasStudentCompletedExamAsync(dto.StudentId, examId))
            .ReturnsAsync(false);
        _mockResultRepository.Setup(r => r.AddAsync(It.IsAny<ExamResult>()))
            .ReturnsAsync((ExamResult r) => r);

        // Act
        var result = await _service.StartExamAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.StudentId, result.StudentId);
        Assert.Equal(examId, result.ExamDefinitionId);
        Assert.Equal(ExamStatus.InProgress, result.Status);
        Assert.Single(result.SectionResults);
        Assert.Equal(85, result.TotalScore);
        Assert.Equal(85, result.Percentage);
    }

    [Fact]
    public async Task StartExamAsync_UnpublishedExam_ThrowsException()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var exam = new ExamDefinition
        {
            Id = examId,
            IsPublished = false
        };

        var dto = new CreateExamResultDto
        {
            StudentId = Guid.NewGuid(),
            ExamDefinitionId = examId,
            StartTime = DateTime.UtcNow,
            SectionResults = new List<CreateSectionResultDto>()
        };

        _mockExamRepository.Setup(r => r.GetByIdWithSectionsAsync(examId))
            .ReturnsAsync(exam);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.StartExamAsync(dto));
    }

    [Fact]
    public async Task UpdateExamResultAsync_ValidDto_UpdatesAndReturnsResult()
    {
        // Arrange
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var exam = new ExamDefinition
        {
            Id = examId,
            TotalPoints = 100,
            Sections = new List<ExamSection>
            {
                new ExamSection
                {
                    Id = sectionId,
                    Points = 100
                }
            }
        };

        var existingResult = new ExamResult
        {
            Id = resultId,
            ExamDefinitionId = examId,
            Status = ExamStatus.InProgress,
            SectionResults = new List<SectionResult>
            {
                new SectionResult
                {
                    ExamSectionId = sectionId,
                    Score = 80,
                    TimeTaken = TimeSpan.FromMinutes(25)
                }
            }
        };

        var updateDto = new UpdateExamResultDto
        {
            EndTime = DateTime.UtcNow,
            Status = ExamStatus.Completed,
            SectionResults = new List<UpdateSectionResultDto>
            {
                new UpdateSectionResultDto
                {
                    ExamSectionId = sectionId,
                    Score = 90,
                    TimeTaken = TimeSpan.FromMinutes(30)
                }
            }
        };

        _mockResultRepository.Setup(r => r.GetByIdWithSectionResultsAsync(resultId))
            .ReturnsAsync(existingResult);
        _mockExamRepository.Setup(r => r.GetByIdWithSectionsAsync(examId))
            .ReturnsAsync(exam);
        _mockResultRepository.Setup(r => r.UpdateAsync(It.IsAny<ExamResult>()))
            .ReturnsAsync((ExamResult r) => r);

        // Act
        var result = await _service.UpdateExamResultAsync(resultId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Status, result.Status);
        Assert.Equal(updateDto.EndTime, result.EndTime);
        Assert.Single(result.SectionResults);
        Assert.Equal(90, result.TotalScore);
        Assert.Equal(90, result.Percentage);
    }
} 