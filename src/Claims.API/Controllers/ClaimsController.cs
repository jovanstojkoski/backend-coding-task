using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.Create;
using Claims.Application.UseCases.Claims.Delete;
using Claims.Application.UseCases.Claims.Get;
using Claims.Application.UseCases.Claims.GetById;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("claims")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class ClaimsController(ILogger<ClaimsController> logger) : ControllerBase
{
    private readonly ILogger<ClaimsController> _logger = logger;

    /// <summary>
    /// Returns a paginated list of claims.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<GetClaimsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] GetClaimsRequest request,
        [FromServices] IGetClaimsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve claims: {Error}", result.Error);
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: result.Error,
                title: "Claims Retrieval Error");
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns a claim by ID.
    /// </summary>
    [HttpGet("{id}", Name = nameof(GetClaimByIdAsync))]
    [ProducesResponseType(typeof(GetClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimByIdAsync(
        [FromRoute] string id,
        [FromServices] IGetClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new GetClaimRequest(id), cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve claim {ClaimId}: {Error}", id, result.Error);
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: result.Error,
                title: "Claim Retrieval Error");
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a claim for an existing cover.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateClaimResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateClaimRequest request,
        [FromServices] ICreateClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create claim: {Error}", result.Error);
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: result.Error,
                title: "Claim Creation Error");
        }

        return CreatedAtRoute(
            nameof(GetClaimByIdAsync),
            new
            {
                id = result.Value.Id
            },
            result.Value);
    }

    /// <summary>
    /// Deletes a claim by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] string id,
        [FromServices] IDeleteClaimUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new DeleteClaimRequest(id), cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to delete claim {ClaimId}: {Error}", id, result.Error);
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: result.Error,
                title: "Claim Deletion Error");
        }

        return NoContent();
    }
}
