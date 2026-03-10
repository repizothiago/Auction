using Auction.Domain.Entities.Base;
using Auction.Domain.Events.Bid;
using Auction.Domain.ValueObjects;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.Entities;

public class Bid : BaseEntity
{
    protected Bid() { }

    private Bid(
        Guid auctionId,
        Guid bidderId,
        Money amount,
        bool isAutoBid = false,
        Guid? bidId = null) 
        : base(bidId ?? Guid.NewGuid(), DateTime.UtcNow, null)
    {
        AuctionId = auctionId;
        BidderId = bidderId;
        Amount = amount;
        IsAutoBid = isAutoBid;
        BidStatus = BidStatus.Active;
    }

    public Guid AuctionId { get; private set; }
    public Guid BidderId { get; private set; }
    public Money Amount { get; private set; }
    public bool IsAutoBid { get; private set; }
    public BidStatus BidStatus { get; private set; }
    public DateTime BidTime { get; private set; }

    public static Result<Bid> Create(
        Guid auctionId,
        Guid bidderId,
        Money amount,
        bool isAutoBid = false)
    {
        if (auctionId == Guid.Empty)
            return Result<Bid>.Failure(new Error("Bid.InvalidAuction", "ID do leilão inválido"));

        if (bidderId == Guid.Empty)
            return Result<Bid>.Failure(new Error("Bid.InvalidBidder", "ID do licitante inválido"));

        if (amount.Value <= 0)
            return Result<Bid>.Failure(new Error("Bid.InvalidAmount", "Valor do lance deve ser maior que zero"));

        var bid = new Bid(auctionId, bidderId, amount, isAutoBid)
        {
            BidTime = DateTime.UtcNow
        };

        bid.RaiseDomainEvent(new BidPlacedEvent(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.IsAutoBid,
            bid.BidTime));

        return Result<Bid>.Success(bid);
    }

    /// <summary>
    /// Cria um Bid com ID pré-gerado (para idempotência no consumer)
    /// </summary>
    public static Result<Bid> Create(
        Guid auctionId,
        Guid bidderId,
        Money amount,
        bool isAutoBid,
        Guid bidId)
    {
        if (bidId == Guid.Empty)
            return Result<Bid>.Failure(new Error("Bid.InvalidId", "ID do lance inválido"));

        if (auctionId == Guid.Empty)
            return Result<Bid>.Failure(new Error("Bid.InvalidAuction", "ID do leilão inválido"));

        if (bidderId == Guid.Empty)
            return Result<Bid>.Failure(new Error("Bid.InvalidBidder", "ID do licitante inválido"));

        if (amount.Value <= 0)
            return Result<Bid>.Failure(new Error("Bid.InvalidAmount", "Valor do lance deve ser maior que zero"));

        var bid = new Bid(auctionId, bidderId, amount, isAutoBid, bidId)
        {
            BidTime = DateTime.UtcNow
        };

        bid.RaiseDomainEvent(new BidPlacedEvent(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.IsAutoBid,
            bid.BidTime));

        return Result<Bid>.Success(bid);
    }

    public Result Cancel()
    {
        if (BidStatus != BidStatus.Active)
            return Result.Failure(new Error("Bid.AlreadyCancelled", "Lance já foi cancelado"));

        BidStatus = BidStatus.Cancelled;
        RaiseDomainEvent(new BidCancelledEvent(Id, AuctionId, BidderId));
        
        return Result.Success();
    }

    public Result Outbid()
    {
        if (BidStatus != BidStatus.Active)
            return Result.Failure(new Error("Bid.NotActive", "Lance não está ativo"));

        BidStatus = BidStatus.Outbid;
        RaiseDomainEvent(new BidOutbidEvent(Id, AuctionId, BidderId, Amount.Value));
        
        return Result.Success();
    }

    public Result Win()
    {
        if (BidStatus != BidStatus.Active)
            return Result.Failure(new Error("Bid.NotActive", "Lance não está ativo"));

        BidStatus = BidStatus.Won;
        
        return Result.Success();
    }
}

public enum BidStatus
{
    Active = 0,      // Lance ativo (maior lance atual)
    Outbid = 1,      // Lance superado por outro
    Cancelled = 2,   // Lance cancelado
    Won = 3          // Lance vencedor (leilão finalizado)
}
