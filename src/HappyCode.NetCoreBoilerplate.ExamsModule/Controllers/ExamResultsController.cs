using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyCode.NetCoreBoilerplate.ExamsModule.DTOs;
using HappyCode.NetCoreBoilerplate.ExamsModule.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Controllers;

/// <summary>
/// Controller for managing exam results including starting exams, updating results, and importing/exporting results.
/// </summary>
[ApiController]
[Route("api/exam-results")]
[Authorize]
public class ExamResultsController : ControllerBase
{
    private readonly IExamResultService _resultService;

    public ExamResultsController(IExamResultService resultService)
    {
        _resultService = resultService;
    }

    /// <summary>
    /// Retrieves a specific exam result by ID.
    /// </summary>
    /// <param name="id">The ID of the exam result to retrieve.</param>
    /// <returns>The requested exam result.</returns>
    /// <response code="200">Returns the requested exam result.</response>
    /// <response code="404">If the exam result is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExamResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResultByIdAsync(Guid id)
    {
        var result = await _resultService.GetResultByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all exam results for a specific student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>A list of exam results for the specified student.</returns>
    /// <response code="200">Returns the list of exam results.</response>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(IEnumerable<ExamResultListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentResultsAsync(Guid studentId)
    {
        var results = await _resultService.GetStudentResultsAsync(studentId);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves all results for a specific exam.
    /// </summary>
    /// <param name="examId">The ID of the exam.</param>
    /// <returns>A list of all results for the specified exam.</returns>
    /// <response code="200">Returns the list of exam results.</response>
    [HttpGet("exam/{examId}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<ExamResultListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamResultsAsync(Guid examId)
    {
        var results = await _resultService.GetExamResultsAsync(examId);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves a specific student's result for a specific exam.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <param name="examId">The ID of the exam.</param>
    /// <returns>The exam result for the specified student and exam.</returns>
    /// <response code="200">Returns the requested exam result.</response>
    /// <response code="404">If the exam result is not found.</response>
    [HttpGet("student/{studentId}/exam/{examId}")]
    [ProducesResponseType(typeof(ExamResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentExamResultAsync(Guid studentId, Guid examId)
    {
        var result = await _resultService.GetStudentExamResultAsync(studentId, examId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Starts a new exam attempt for a student.
    /// </summary>
    /// <param name="dto">The exam start data including student ID and exam ID.</param>
    /// <returns>The newly created exam result.</returns>
    /// <response code="201">Returns the newly created exam result.</response>
    /// <response code="400">If the request data is invalid or the exam cannot be started.</response>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ExamResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartExamAsync(CreateExamResultDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _resultService.StartExamAsync(dto);
            return CreatedAtAction(nameof(GetResultByIdAsync), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing exam result.
    /// </summary>
    /// <param name="id">The ID of the exam result to update.</param>
    /// <param name="dto">The updated exam result data.</param>
    /// <returns>The updated exam result.</returns>
    /// <response code="200">Returns the updated exam result.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the exam result is not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ExamResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateExamResultAsync(Guid id, UpdateExamResultDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _resultService.UpdateExamResultAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Exports exam results to an Excel file.
    /// </summary>
    /// <param name="examId">The ID of the exam to export results for.</param>
    /// <returns>An Excel file containing the exam results.</returns>
    /// <response code="200">Returns the Excel file containing exam results.</response>
    /// <response code="404">If the exam is not found.</response>
    [HttpGet("exam/{examId}/export")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportExamResultsAsync(Guid examId)
    {
        try
        {
            var fileContent = await _resultService.ExportExamResultsToExcelAsync(examId);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"exam-results-{examId}.xlsx");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Imports exam results from an Excel file.
    /// </summary>
    /// <param name="examId">The ID of the exam to import results for.</param>
    /// <param name="file">The Excel file containing exam results.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">If the results were successfully imported.</response>
    /// <response code="400">If the file is invalid or import fails.</response>
    /// <response code="404">If the exam is not found.</response>
    [HttpPost("exam/{examId}/import")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportExamResultsAsync(Guid examId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (!file.FileName.EndsWith(".xlsx"))
            return BadRequest("Only Excel files (.xlsx) are supported");

        try
        {
            using var stream = new System.IO.MemoryStream();
            await file.CopyToAsync(stream);
            var success = await _resultService.ImportExamResultsFromExcelAsync(examId, stream.ToArray());
            return success ? Ok() : BadRequest("Failed to import results");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error importing results: {ex.Message}");
        }
    }
} 