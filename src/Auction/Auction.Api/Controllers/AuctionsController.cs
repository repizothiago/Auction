using Auction.Api.Extensions;
using Auction.Application.Commands;
using Auction.Application.Commands.Auction;
using Auction.Application.DTOs;
using Auction.Application.Queries.Auction;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Controllers;

/// <summary>
/// Controller para operações REST de leilões
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuctionsController : ControllerBase
{
    private readonly ICommandHandler<CancelAuctionCommand> _cancelAuctionHandler;
    private readonly GetAllAuctionsQueryHandler _getAllAuctionsQueryHandler;
    private readonly ILogger<AuctionsController> _logger;

    public AuctionsController(
        ICommandHandler<CancelAuctionCommand> cancelAuctionHandler,
        GetAllAuctionsQueryHandler getAllAuctionsQueryHandler,
        ILogger<AuctionsController> logger)
    {
        _cancelAuctionHandler = cancelAuctionHandler;
        _getAllAuctionsQueryHandler = getAllAuctionsQueryHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os leilões com paginação
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de leilões</returns>
    /// <response code="200">Lista de leilões retornada com sucesso</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Domain.Entities.Auction>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAuctions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllAuctionsQuery(pageNumber, pageSize);
        var auctions = await _getAllAuctionsQueryHandler.HandleAsync(query, cancellationToken);

        return Ok(auctions);
    }

    /// <summary>
    /// Cancela um leilão específico
    /// </summary>
    /// <param name="id">ID do leilão</param>
    /// <param name="request">Dados do cancelamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status da operação</returns>
    /// <response code="204">Leilão cancelado com sucesso</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada</response>
    /// <response code="404">Leilão não encontrado</response>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAuction(
        Guid id,
        [FromBody] CancelAuctionRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Received request to cancel auction {AuctionId} with reason: {Reason}",
            id,
            request.Reason);

        var command = new CancelAuctionCommand(id, request.Reason);
        var result = await _cancelAuctionHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var problemDetails = result.ToProblemDetails();
            return problemDetails.Status switch
            {
                StatusCodes.Status404NotFound => NotFound(problemDetails),
                _ => BadRequest(problemDetails)
            };
        }

        return NoContent();
    }
}
