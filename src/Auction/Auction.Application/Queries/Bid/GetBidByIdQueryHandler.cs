using Auction.Application.DTOs;
using Auction.Application.Interfaces.Repositories;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace Auction.Application.Queries.Bid;

/// <summary>
/// Handler para buscar um lance por ID
/// </summary>
public class GetBidByIdQueryHandler
{
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<GetBidByIdQueryHandler> _logger;

    public GetBidByIdQueryHandler(
        IBidRepository bidRepository,
        ILogger<GetBidByIdQueryHandler> logger)
    {
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public async Task<Result<BidDto>> HandleAsync(
        GetBidByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching bid {BidId}", query.BidId);

        var bid = await _bidRepository.GetByIdAsync(query.BidId, cancellationToken);

        if (bid is null)
        {
            return Result<BidDto>.Failure(
                Error.NotFound("Bid.NotFound", $"Lance {query.BidId} não encontrado"));
        }

        var dto = new BidDto(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.Amount.Currency,
            bid.IsAutoBid,
            bid.BidStatus.ToString(),
            bid.BidTime);

        return Result<BidDto>.Success(dto);
    }
}
