using System;
using System.Collections.Generic;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;

public class ExamResultDto : BaseDto
{
    public Guid StudentId { get; set; }
    public Guid ExamDefinitionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalScore { get; set; }
    public decimal Percentage { get; set; }
    public ExamStatus Status { get; set; }
    public List<SectionResultDto> SectionResults { get; set; }
}

public class SectionResultDto : BaseDto
{
    public Guid ExamSectionId { get; set; }
    public int Score { get; set; }
    public decimal Percentage { get; set; }
    public TimeSpan TimeTaken { get; set; }
}

public class CreateExamResultDto
{
    public Guid StudentId { get; set; }
    public Guid ExamDefinitionId { get; set; }
    public DateTime StartTime { get; set; }
    public List<CreateSectionResultDto> SectionResults { get; set; }
}

public class CreateSectionResultDto
{
    public Guid ExamSectionId { get; set; }
    public int Score { get; set; }
    public TimeSpan TimeTaken { get; set; }
}

public class UpdateExamResultDto
{
    public DateTime? EndTime { get; set; }
    public ExamStatus Status { get; set; }
    public List<UpdateSectionResultDto> SectionResults { get; set; }
}

public class UpdateSectionResultDto
{
    public Guid ExamSectionId { get; set; }
    public int Score { get; set; }
    public TimeSpan TimeTaken { get; set; }
}

public class ExamResultListItemDto
{
    public Guid Id { get; set; }
    public string ExamTitle { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalScore { get; set; }
    public decimal Percentage { get; set; }
    public ExamStatus Status { get; set; }
} 