using System;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;

public class ExamSectionDto : BaseDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public int QuestionCount { get; set; }
    public TimeSpan SuggestedDuration { get; set; }
}

public class CreateExamSectionDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public int QuestionCount { get; set; }
    public TimeSpan SuggestedDuration { get; set; }
}

public class UpdateExamSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public int QuestionCount { get; set; }
    public TimeSpan SuggestedDuration { get; set; }
} 