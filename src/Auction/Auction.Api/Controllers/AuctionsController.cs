using Auction.Application.Commands;
using Auction.Application.Commands.Auction;
using Auction.Application.DTOs;
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
    private readonly ILogger<AuctionsController> _logger;

    public AuctionsController(
        ICommandHandler<CancelAuctionCommand> cancelAuctionHandler,
        ILogger<AuctionsController> logger)
    {
        _cancelAuctionHandler = cancelAuctionHandler;
        _logger = logger;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            var error = result.Error!;

            // Verificar tipo de erro
            if (error.Code == "Auction.NotFound")
            {
                return NotFound(new { error.Code, error.Message });
            }

            return BadRequest(new { error.Code, error.Message });
        }

        return NoContent();
    }
}
