using HappyCode.NetCoreBoilerplate.Core;
using HappyCode.NetCoreBoilerplate.Core.Dtos;
using HappyCode.NetCoreBoilerplate.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace HappyCode.NetCoreBoilerplate.Api.Controllers
{
    /// <summary>
    /// Manages employee-related operations
    /// </summary>
    [FeatureGate(FeatureFlags.DockerCompose)]
    [Route("api/employees")]
    public class EmployeesController : ApiControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IFeatureManager _featureManager;

        public EmployeesController(IEmployeeRepository employeeRepository, IFeatureManager featureManager)
        {
            _employeeRepository = employeeRepository;
            _featureManager = featureManager;
        }

        /// <summary>
        /// Gets all employees
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all employees</returns>
        /// <response code="200">Returns the list of employees</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var result = await _employeeRepository.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific employee by ID
        /// </summary>
        /// <param name="id">The employee ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The employee details</returns>
        /// <response code="200">Returns the employee</response>
        /// <response code="404">If the employee is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _employeeRepository.GetByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific employee with detailed information
        /// </summary>
        /// <param name="id">The employee ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The employee with detailed information</returns>
        /// <response code="200">Returns the employee details</response>
        /// <response code="404">If the employee is not found</response>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(EmployeeDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWithDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _employeeRepository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Gets the oldest employee (with Santa feature flag)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The oldest employee</returns>
        /// <response code="200">Returns the oldest employee</response>
        /// <response code="404">If no employees are found</response>
        [HttpGet("oldest")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOldestAsync(
            CancellationToken cancellationToken = default)
        {
            if (await _featureManager.IsEnabledAsync(FeatureFlags.Santa.ToString()))
            {
                return Ok(new EmployeeDto
                {
                    Id = int.MaxValue,
                    FirstName = "Santa",
                    LastName = "Claus",
                    BirthDate = new DateTime(270, 3, 15),
                    Gender = "M",
                });
            }

            var result = await _employeeRepository.GetOldestAsync(cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing employee
        /// </summary>
        /// <param name="id">The employee ID</param>
        /// <param name="employeePutDto">The updated employee data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated employee</returns>
        /// <response code="200">Returns the updated employee</response>
        /// <response code="400">If the data is invalid</response>
        /// <response code="404">If the employee is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EmployeePutDto employeePutDto,
            CancellationToken cancellationToken = default)
        {
            var result = await _employeeRepository.UpdateAsync(id, employeePutDto, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Creates a new employee
        /// </summary>
        /// <param name="employeePostDto">The employee data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created employee</returns>
        /// <response code="201">Returns the created employee</response>
        /// <response code="400">If the data is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAsync(
            [FromBody] EmployeePostDto employeePostDto,
            CancellationToken cancellationToken = default)
        {
            var result = await _employeeRepository.InsertAsync(employeePostDto, cancellationToken);
            Response.Headers.Append("x-date-created", DateTime.UtcNow.ToString("s"));
            return CreatedAtAction("Get", new { id = result.Id }, result);
        }

        /// <summary>
        /// Deletes an employee
        /// </summary>
        /// <param name="id">The employee ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        /// <response code="204">If the employee was deleted successfully</response>
        /// <response code="404">If the employee is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _employeeRepository.DeleteByIdAsync(id, cancellationToken);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
