using System;
using System.Collections.Generic;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;

public class ExamDefinitionDto : BaseDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
    public int TotalPoints { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ExamType Type { get; set; }
    public bool IsPublished { get; set; }
    public List<ExamSectionDto> Sections { get; set; }
}

public class CreateExamDefinitionDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ExamType Type { get; set; }
    public List<CreateExamSectionDto> Sections { get; set; }
}

public class UpdateExamDefinitionDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<UpdateExamSectionDto> Sections { get; set; }
}

public class ExamDefinitionListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public ExamType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublished { get; set; }
    public int TotalPoints { get; set; }
} 