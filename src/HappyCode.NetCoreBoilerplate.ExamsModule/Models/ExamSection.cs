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
    public DateTime CreatedAt { get; set; }

    public int ExamDefinitionId { get; set; }
    public required ExamDefinition ExamDefinition { get; set; }
} 