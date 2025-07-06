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

public class ExamDefinitionServiceTests
{
    private readonly Mock<IExamDefinitionRepository> _mockRepository;
    private readonly ExamDefinitionService _service;

    public ExamDefinitionServiceTests()
    {
        _mockRepository = new Mock<IExamDefinitionRepository>();
        _service = new ExamDefinitionService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetExamByIdAsync_ExistingExam_ReturnsExamDto()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var exam = new ExamDefinition
        {
            Id = examId,
            Title = "Test Exam",
            Description = "Test Description",
            Type = ExamType.Quiz,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            IsPublished = true,
            Sections = new List<ExamSection>
            {
                new ExamSection
                {
                    Id = Guid.NewGuid(),
                    Title = "Section 1",
                    Points = 10,
                    OrderIndex = 1
                }
            }
        };

        _mockRepository.Setup(r => r.GetByIdWithSectionsAsync(examId))
            .ReturnsAsync(exam);

        // Act
        var result = await _service.GetExamByIdAsync(examId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(examId, result.Id);
        Assert.Equal(exam.Title, result.Title);
        Assert.Equal(exam.Type, result.Type);
        Assert.Single(result.Sections);
    }

    [Fact]
    public async Task CreateExamAsync_ValidDto_CreatesAndReturnsExam()
    {
        // Arrange
        var createdById = Guid.NewGuid();
        var dto = new CreateExamDefinitionDto
        {
            Title = "New Exam",
            Description = "New Description",
            Type = ExamType.Quiz,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Sections = new List<CreateExamSectionDto>
            {
                new CreateExamSectionDto
                {
                    Title = "Section 1",
                    Points = 10,
                    OrderIndex = 1
                }
            }
        };

        ExamDefinition savedExam = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ExamDefinition>()))
            .Callback<ExamDefinition>(e => savedExam = e)
            .ReturnsAsync((ExamDefinition e) => e);

        // Act
        var result = await _service.CreateExamAsync(dto, createdById);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Type, result.Type);
        Assert.Single(result.Sections);
        Assert.False(result.IsPublished);
        Assert.Equal(createdById, savedExam.CreatedById);
    }

    [Fact]
    public async Task UpdateExamAsync_ValidDto_UpdatesAndReturnsExam()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var existingExam = new ExamDefinition
        {
            Id = examId,
            Title = "Old Title",
            Description = "Old Description",
            Type = ExamType.Quiz,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Sections = new List<ExamSection>
            {
                new ExamSection
                {
                    Id = Guid.NewGuid(),
                    Title = "Old Section",
                    Points = 5,
                    OrderIndex = 1
                }
            }
        };

        var updateDto = new UpdateExamDefinitionDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Sections = new List<UpdateExamSectionDto>
            {
                new UpdateExamSectionDto
                {
                    Id = existingExam.Sections.First().Id,
                    Title = "Updated Section",
                    Points = 10,
                    OrderIndex = 1
                }
            }
        };

        _mockRepository.Setup(r => r.GetByIdWithSectionsAsync(examId))
            .ReturnsAsync(existingExam);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ExamDefinition>()))
            .ReturnsAsync((ExamDefinition e) => e);

        // Act
        var result = await _service.UpdateExamAsync(examId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Title, result.Title);
        Assert.Equal(updateDto.Description, result.Description);
        Assert.Single(result.Sections);
        Assert.Equal(updateDto.Sections[0].Title, result.Sections[0].Title);
        Assert.Equal(updateDto.Sections[0].Points, result.Sections[0].Points);
    }

    [Fact]
    public async Task PublishExamAsync_ExistingExam_PublishesExam()
    {
        // Arrange
        var examId = Guid.NewGuid();
        _mockRepository.Setup(r => r.PublishExamAsync(examId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.PublishExamAsync(examId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.PublishExamAsync(examId), Times.Once);
    }
} 