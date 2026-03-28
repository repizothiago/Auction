using Auction.Api.Extensions;
using Auction.Api.Models.Requests;
using Auction.Application.CommandHandlers.Bid;
using Auction.Application.Commands.Bid;
using Auction.Application.DTOs;
using Auction.Application.Queries.Bid;
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
    private readonly GetBidByIdQueryHandler _getBidByIdHandler;
    private readonly GetBidsByAuctionQueryHandler _getBidsByAuctionHandler;
    private readonly GetHighestBidQueryHandler _getHighestBidHandler;
    private readonly ILogger<BidsController> _logger;

    public BidsController(
        PlaceBidCommandHandler placeBidHandler,
        GetBidByIdQueryHandler getBidByIdHandler,
        GetBidsByAuctionQueryHandler getBidsByAuctionHandler,
        GetHighestBidQueryHandler getHighestBidHandler,
        ILogger<BidsController> logger)
    {
        _placeBidHandler = placeBidHandler;
        _getBidByIdHandler = getBidByIdHandler;
        _getBidsByAuctionHandler = getBidsByAuctionHandler;
        _getHighestBidHandler = getHighestBidHandler;
        _logger = logger;
    }

    /// <summary>
    /// Criar um novo lance em um leilão
    /// </summary>
    /// <param name="auctionId">ID do leilão</param>
    /// <param name="request">Dados do lance</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>ID do lance criado</returns>
    /// <response code="202">Lance aceito para processamento assíncrono</response>
    /// <response code="400">Dados inválidos ou regras de negócio violadas</response>
    /// <response code="404">Leilão não encontrado</response>
    /// <response code="422">Validação falhou</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PlaceBid(
        [FromRoute] Guid auctionId,
        [FromBody] PlaceBidRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Obter BidderId do usuário autenticado (Claims)
        // Por enquanto, usando um ID fictício para desenvolvimento
        var bidderId = GetCurrentUserId() ?? Guid.Parse("10000000-0000-0000-0000-000000000001");

        var command = new PlaceBidCommand(
            auctionId,
            bidderId,
            request.Bid,
            request.IdempotencyKey);

        _logger.LogInformation(
            "[Lance] Solicitação de lance recebida: AuctionId={AuctionId}, LicitanteId={LicitanteId}, Valor={Valor}",
            auctionId, bidderId, request.Bid.Value);

        var result = await _placeBidHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var problemDetails = result.ToProblemDetails();
            _logger.LogWarning(
                "[Lance] Lance rejeitado: AuctionId={AuctionId}, Motivo={Motivo}",
                auctionId, problemDetails.Detail);
            return problemDetails.Status switch
            {
                StatusCodes.Status404NotFound => NotFound(problemDetails),
                StatusCodes.Status422UnprocessableEntity => UnprocessableEntity(problemDetails),
                _ => BadRequest(problemDetails)
            };
        }

        return AcceptedAtAction(
            nameof(GetBidById),
            new { auctionId, bidId = result.Value },
            new
            {
                bidId = result.Value,
                message = "Lance aceito e será processado em breve",
                statusUrl = Url.Action(nameof(GetBidById), new { auctionId, bidId = result.Value })
            });
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
    [ProducesResponseType(typeof(List<BidDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBidsByAuction(
        [FromRoute] Guid auctionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBidsByAuctionQuery(auctionId, pageNumber, pageSize);
        var bids = await _getBidsByAuctionHandler.HandleAsync(query, cancellationToken);

        return Ok(bids);
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
    [ProducesResponseType(typeof(BidDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBidById(
        [FromRoute] Guid auctionId,
        [FromRoute] Guid bidId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBidByIdQuery(bidId);
        var result = await _getBidByIdHandler.HandleAsync(query, cancellationToken);

        if (!result.IsSuccess)
        {
            var problemDetails = result.ToProblemDetails();
            return NotFound(problemDetails);
        }

        // Validar se o bid pertence ao leilão correto
        if (result.Value.AuctionId != auctionId)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Bid.NotInAuction",
                Detail = $"Lance {bidId} não pertence ao leilão {auctionId}",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result.Value);
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
    [ProducesResponseType(typeof(BidDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHighestBid(
        [FromRoute] Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetHighestBidQuery(auctionId);
        var result = await _getHighestBidHandler.HandleAsync(query, cancellationToken);

        if (!result.IsSuccess)
        {
            var problemDetails = result.ToProblemDetails();
            return NotFound(problemDetails);
        }

        return Ok(result.Value);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
