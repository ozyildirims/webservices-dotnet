using System;
using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Models;

public class ExamSection : BaseEntity
{
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }

    public int MaxScore { get; set; }
    public int Order { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public int QuestionCount { get; set; }
    public TimeSpan SuggestedDuration { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid ExamDefinitionId { get; set; }
    public ExamDefinition? ExamDefinition { get; set; }
} 