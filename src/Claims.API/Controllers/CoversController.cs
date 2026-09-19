using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Covers.ComputePremium;
using Claims.Application.UseCases.Covers.Create;
using Claims.Application.UseCases.Covers.Delete;
using Claims.Application.UseCases.Covers.Get;
using Claims.Application.UseCases.Covers.GetById;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("covers")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class CoversController(ILogger<CoversController> logger) : ControllerBase
{
    private readonly ILogger<CoversController> _logger = logger;

    /// <summary>
    /// Computes the premium for an insurance period and cover type.
    /// </summary>
    [HttpPost("compute")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ComputePremiumAsync(
        [FromQuery] ComputePremiumRequest request,
        [FromServices] IComputePremiumUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to compute premium: {Error}", result.Error);
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: result.Error,
                title: "Premium Calculation Error");
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns a paginated list of covers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<GetCoversResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] GetCoversRequest request,
        [FromServices] IGetCoversUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve covers: {Error}", result.Error);
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: result.Error,
                title: "Covers Retrieval Error");
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns a cover by ID.
    /// </summary>
    [HttpGet("{id}", Name = nameof(GetCoverByIdAsync))]
    [ProducesResponseType(typeof(GetCoverResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoverByIdAsync(
        [FromRoute] string id,
        [FromServices] IGetCoverUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new GetCoverRequest(id), cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve cover {CoverId}: {Error}", id, result.Error);
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: result.Error,
                title: "Cover Retrieval Error");
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a cover and computes its premium.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCoverResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCoverRequest request,
        [FromServices] ICreateCoverUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create cover: {Error}", result.Error);
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: result.Error,
                title: "Cover Creation Error");
        }

        return CreatedAtRoute(
            nameof(GetCoverByIdAsync),
            new
            {
                id = result.Value.Id
            },
            result.Value);
    }

    /// <summary>
    /// Deletes a cover by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] string id,
        [FromServices] IDeleteCoverUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new DeleteCoverRequest(id), cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to delete cover {CoverId}: {Error}", id, result.Error);
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: result.Error,
                title: "Cover Deletion Error");
        }

        return NoContent();
    }
}
