using System;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;

public abstract class BaseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 