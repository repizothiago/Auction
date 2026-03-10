using Auction.Api.Models.Requests;
using Auction.Api.Models.Responses;
using Auction.Application.CommandHandlers.Bid;
using Auction.Application.Commands.Bid;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auction.Api.Controllers;

/// <summary>
/// Controller para operações de lances (bids)
/// </summary>
[ApiController]
[Route("api/auctions/{auctionId:guid}/bids")]
[Produces("application/json")]
public class BidsController : ControllerBase
{
    private readonly PlaceBidCommandHandler _placeBidHandler;
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<BidsController> _logger;

    public BidsController(
        PlaceBidCommandHandler placeBidHandler,
        IBidRepository bidRepository,
        ILogger<BidsController> logger)
    {
        _placeBidHandler = placeBidHandler;
        _bidRepository = bidRepository;
        _logger = logger;
    }

    /// <summary>
    /// Criar um novo lance em um leilão
    /// </summary>
    /// <param name="auctionId">ID do leilão</param>
    /// <param name="request">Dados do lance</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>ID do lance criado</returns>
    /// <response code="201">Lance criado com sucesso</response>
    /// <response code="400">Dados inválidos ou regras de negócio violadas</response>
    /// <response code="404">Leilão não encontrado</response>
    /// <response code="409">Conflito de concorrência (outro lance foi registrado)</response>
    [HttpPost]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PlaceBid(
        [FromRoute] Guid auctionId,
        [FromBody] PlaceBidRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Obter BidderId do usuário autenticado (Claims)
        // Por enquanto, usando um ID fictício para desenvolvimento
        var bidderId = GetCurrentUserId() ?? Guid.Parse("11111111-1111-1111-1111-111111111111");

        var command = new PlaceBidCommand(
            auctionId,
            bidderId,
            request.Amount,
            request.IdempotencyKey);

        var result = await _placeBidHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Bid.AuctionNotFound" => NotFound(new ProblemDetails
                {
                    Title = "Leilão não encontrado",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status404NotFound
                }),
                "Bid.ConcurrencyConflict" => Conflict(new ProblemDetails
                {
                    Title = "Conflito de concorrência",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status409Conflict
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Erro ao processar lance",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status400BadRequest
                })
            };
        }

        // Buscar bid criado para retornar na resposta
        var bid = await _bidRepository.GetByIdAsync(result.Value, cancellationToken);
        if (bid is null)
        {
            return StatusCode(500, new ProblemDetails
            {
                Title = "Erro interno",
                Detail = "Lance criado mas não foi possível recuperar os dados",
                Status = StatusCodes.Status500InternalServerError
            });
        }

        var response = new BidResponse(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.Amount.Currency,
            bid.IsAutoBid,
            bid.BidStatus.ToString(),
            bid.BidTime);

        return CreatedAtAction(
            nameof(GetBidById),
            new { auctionId = bid.AuctionId, bidId = bid.Id },
            response);
    }

    /// <summary>
    /// Obter histórico de lances de um leilão
    /// </summary>
    /// <param name="auctionId">ID do leilão</param>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de lances</returns>
    /// <response code="200">Lista de lances retornada com sucesso</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<BidResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBidsByAuction(
        [FromRoute] Guid auctionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var bids = await _bidRepository.GetByAuctionIdAsync(
            auctionId, 
            pageNumber, 
            pageSize, 
            cancellationToken);

        var response = bids.Select(bid => new BidResponse(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.Amount.Currency,
            bid.IsAutoBid,
            bid.BidStatus.ToString(),
            bid.BidTime)).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Obter um lance específico
    /// </summary>
    /// <param name="auctionId">ID do leilão</param>
    /// <param name="bidId">ID do lance</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do lance</returns>
    /// <response code="200">Lance encontrado</response>
    /// <response code="404">Lance não encontrado</response>
    [HttpGet("{bidId:guid}", Name = nameof(GetBidById))]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBidById(
        [FromRoute] Guid auctionId,
        [FromRoute] Guid bidId,
        CancellationToken cancellationToken = default)
    {
        var bid = await _bidRepository.GetByIdAsync(bidId, cancellationToken);

        if (bid is null || bid.AuctionId != auctionId)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Lance não encontrado",
                Detail = $"Lance {bidId} não encontrado no leilão {auctionId}",
                Status = StatusCodes.Status404NotFound
            });
        }

        var response = new BidResponse(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.Amount.Currency,
            bid.IsAutoBid,
            bid.BidStatus.ToString(),
            bid.BidTime);

        return Ok(response);
    }

    /// <summary>
    /// Obter o lance vencedor de um leilão
    /// </summary>
    /// <param name="auctionId">ID do leilão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lance vencedor</returns>
    /// <response code="200">Lance vencedor encontrado</response>
    /// <response code="404">Nenhum lance encontrado</response>
    [HttpGet("highest")]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHighestBid(
        [FromRoute] Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        var bid = await _bidRepository.GetHighestBidForAuctionAsync(auctionId, cancellationToken);

        if (bid is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Nenhum lance encontrado",
                Detail = $"Nenhum lance ativo encontrado para o leilão {auctionId}",
                Status = StatusCodes.Status404NotFound
            });
        }

        var response = new BidResponse(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.Amount.Currency,
            bid.IsAutoBid,
            bid.BidStatus.ToString(),
            bid.BidTime);

        return Ok(response);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
