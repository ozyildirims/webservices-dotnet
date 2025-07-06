using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;
using HappyCode.NetCoreBoilerplate.ExamsModule.Models;
using HappyCode.NetCoreBoilerplate.ExamsModule.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Controllers;

/// <summary>
/// Controller for managing exam definitions including creation, updates, and publishing.
/// </summary>
[ApiController]
[Route("api/exams")]
[Authorize]
public class ExamDefinitionsController : ControllerBase
{
    private readonly IExamDefinitionService _examService;

    public ExamDefinitionsController(IExamDefinitionService examService)
    {
        _examService = examService;
    }

    /// <summary>
    /// Retrieves all exam definitions.
    /// </summary>
    /// <returns>A list of all exam definitions in the system.</returns>
    /// <response code="200">Returns the list of exam definitions.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExamDefinitionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExamsAsync()
    {
        var exams = await _examService.GetAllExamsAsync();
        return Ok(exams);
    }

    /// <summary>
    /// Retrieves all currently active exam definitions.
    /// </summary>
    /// <returns>A list of active exam definitions.</returns>
    /// <response code="200">Returns the list of active exam definitions.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<ExamDefinitionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveExamsAsync()
    {
        var exams = await _examService.GetActiveExamsAsync();
        return Ok(exams);
    }

    /// <summary>
    /// Retrieves exam definitions by type.
    /// </summary>
    /// <param name="type">The type of exam to filter by (Quiz, Midterm, Final, MockExam).</param>
    /// <returns>A list of exam definitions of the specified type.</returns>
    /// <response code="200">Returns the list of exam definitions of the specified type.</response>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<ExamDefinitionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamsByTypeAsync(ExamType type)
    {
        var exams = await _examService.GetExamsByTypeAsync(type);
        return Ok(exams);
    }

    /// <summary>
    /// Retrieves a specific exam definition by ID.
    /// </summary>
    /// <param name="id">The ID of the exam definition to retrieve.</param>
    /// <returns>The requested exam definition.</returns>
    /// <response code="200">Returns the requested exam definition.</response>
    /// <response code="404">If the exam definition is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExamDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExamByIdAsync(Guid id)
    {
        var exam = await _examService.GetExamByIdAsync(id);
        if (exam == null) return NotFound();
        return Ok(exam);
    }

    /// <summary>
    /// Creates a new exam definition.
    /// </summary>
    /// <param name="dto">The exam definition data.</param>
    /// <returns>The newly created exam definition.</returns>
    /// <response code="201">Returns the newly created exam definition.</response>
    /// <response code="400">If the request data is invalid.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(ExamDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExamAsync(CreateExamDefinitionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new InvalidOperationException("User ID not found"));
        var exam = await _examService.CreateExamAsync(dto, userId);
        
        return CreatedAtAction(nameof(GetExamByIdAsync), new { id = exam.Id }, exam);
    }

    /// <summary>
    /// Updates an existing exam definition.
    /// </summary>
    /// <param name="id">The ID of the exam definition to update.</param>
    /// <param name="dto">The updated exam definition data.</param>
    /// <returns>The updated exam definition.</returns>
    /// <response code="200">Returns the updated exam definition.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the exam definition is not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(ExamDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateExamAsync(Guid id, UpdateExamDefinitionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var exam = await _examService.UpdateExamAsync(id, dto);
        if (exam == null) return NotFound();
        
        return Ok(exam);
    }

    /// <summary>
    /// Deletes an exam definition.
    /// </summary>
    /// <param name="id">The ID of the exam definition to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">If the exam definition was successfully deleted.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteExamAsync(Guid id)
    {
        await _examService.DeleteExamAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Publishes an exam definition, making it available for students.
    /// </summary>
    /// <param name="id">The ID of the exam definition to publish.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">If the exam definition was successfully published.</response>
    /// <response code="404">If the exam definition is not found.</response>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishExamAsync(Guid id)
    {
        var success = await _examService.PublishExamAsync(id);
        if (!success) return NotFound();
        return Ok();
    }

    /// <summary>
    /// Unpublishes an exam definition, making it unavailable for students.
    /// </summary>
    /// <param name="id">The ID of the exam definition to unpublish.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">If the exam definition was successfully unpublished.</response>
    /// <response code="404">If the exam definition is not found.</response>
    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnpublishExamAsync(Guid id)
    {
        var success = await _examService.UnpublishExamAsync(id);
        if (!success) return NotFound();
        return Ok();
    }
} 