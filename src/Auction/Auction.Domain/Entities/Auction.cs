using Auction.Domain.Entities.Base;
using Auction.Domain.Enum;
using Auction.Domain.Events.Auction;
using Auction.Domain.ValueObjects;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.Entities;

public class Auction : BaseEntity
{
    protected Auction() { }

    private Auction(string title, string description, Money startingPrice, Money reservedPrice, Money bidIncrement, DateTime startTime, DateTime endTime, Guid sellerId, Category category, AuctionRules rules) : base(Guid.NewGuid(), DateTime.UtcNow, null)
    {
        Title = title;
        Description = description;
        StartingPrice = startingPrice;
        ReservePrice = reservedPrice;
        BidIncrement = bidIncrement;
        CurrentPrice = startingPrice;
        StartDate = startTime;
        EndDate = endTime;
        SellerId = sellerId;
        Category = category;
        Rules = rules;
    }

    public string Title { get; private set; }
    public string Description { get; private set; }
    public Money StartingPrice { get; private set; }
    public Money ReservePrice { get; private set; }
    public Money? BuyNowPrice { get; private set; }
    public Money CurrentPrice { get; private set; }
    public Money BidIncrement { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public AuctionStatus Status { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid? WinnerId { get; private set; }
    public Category Category { get; private set; }
    public AuctionRules Rules { get; private set; }
    public int TotalBids { get; private set; }
    public Guid? CurrentWinningBidId { get; private set; }

    public static Result<Auction> Create(
      string title,
      string description,
      Money startingPrice,
      Money reservePrice,
      Money bidIncrement,
      DateTime startDate,
      DateTime endDate,
      Guid sellerId,
      Category category,
      AuctionRules? rules = null)
    {
        // Validações
        if (string.IsNullOrWhiteSpace(title))
            return Result<Auction>.Failure(
                new Error("Auction.InvalidTitle", "Título é obrigatório"));

        if (title.Length > 255)
            return Result<Auction>.Failure(
                new Error("Auction.TitleTooLong", "Título deve ter no máximo 255 caracteres"));

        if (endDate <= startDate)
            return Result<Auction>.Failure(
                new Error("Auction.InvalidDates", "Data fim deve ser posterior à data início"));

        if (startDate <= DateTime.UtcNow)
            return Result<Auction>.Failure(
                new Error("Auction.InvalidStartDate", "Data início deve ser futura"));

        if (!reservePrice.IsGreaterThanOrEqual(startingPrice).Value)
            return Result<Auction>.Failure(
                new Error("Auction.InvalidReserve", "Preço reserva deve ser >= preço inicial"));

        var auction = new Auction(
            title, description, startingPrice, reservePrice, bidIncrement,
            startDate, endDate, sellerId, category, rules ?? AuctionRules.CreateDefault());

        auction.RaiseDomainEvent(new AuctionCreatedEvent(auction.Id, auction.Title, auction.StartingPrice.Value));

        return Result<Auction>.Success(auction);
    }


    public Result Schedule()
    {
        if (Status != AuctionStatus.Draft)
            return Result.Failure(
                new Error("Auction.InvalidStatus", "Leilão deve estar em rascunho"));

        Status = AuctionStatus.Scheduled;
        return Result.Success();
    }

    public Result Start()
    {
        if (Status != AuctionStatus.Scheduled)
            return Result.Failure(
                new Error("Auction.InvalidStatus", "Leilão deve estar agendado"));

        if (DateTime.UtcNow < StartDate)
            return Result.Failure(
                new Error("Auction.NotYetStarted", "Leilão ainda não começou"));

        Status = AuctionStatus.Active;
        RaiseDomainEvent(new AuctionStartedEvent(Id));

        return Result.Success();
    }

    public Result RegisterBid(Guid bidId, Money amount, Guid bidderId)
    {
        if (Status != AuctionStatus.Active)
            return Result.Failure(
                new Error("Auction.NotActive", "Leilão não está ativo"));

        if (bidderId == SellerId)
            return Result.Failure(
                new Error("Auction.SellerCannotBid", "Vendedor não pode dar lances no próprio leilão"));

        // Validar valor mínimo do lance
        var minimumBid = CurrentPrice.Add(BidIncrement);
        if (!amount.IsGreaterThanOrEqual(minimumBid).Value)
            return Result.Failure(
                new Error("Auction.BidTooLow", 
                    $"Lance deve ser no mínimo {minimumBid.Value} {minimumBid.Currency}"));

        // Validar Buy Now Price
        if (BuyNowPrice is not null && amount.IsGreaterThanOrEqual(BuyNowPrice).Value)
        {
            // Compra direta - finalizar leilão imediatamente
            CurrentPrice = BuyNowPrice;
            CurrentWinningBidId = bidId;
            WinnerId = bidderId;
            TotalBids++;
            Status = AuctionStatus.Ended;
            RaiseDomainEvent(new AuctionEndedEvent(Id, bidderId, true, BuyNowPrice));
            return Result.Success();
        }

        // Atualizar estado do leilão
        CurrentPrice = amount;
        CurrentWinningBidId = bidId;
        WinnerId = bidderId;
        TotalBids++;

        // Extensão automática se lance nos últimos minutos
        var timeToEnd = EndDate - DateTime.UtcNow;
        if (timeToEnd <= Rules.ExtensionWindow && timeToEnd > TimeSpan.Zero)
        {
            EndDate = EndDate.Add(Rules.ExtensionTime);
            RaiseDomainEvent(new AuctionExtendedEvent(Id, EndDate));
        }

        return Result.Success();
    }

    public Result End()
    {
        if (Status != AuctionStatus.Active)
            return Result.Failure(
                new Error("Auction.NotActive", "Leilão não está ativo"));

        if (DateTime.UtcNow < EndDate)
            return Result.Failure(
                new Error("Auction.NotEnded", "Leilão ainda não terminou"));

        Status = AuctionStatus.Ended;

        // Verificar se atingiu o preço de reserva
        var hasWinner = CurrentPrice.IsGreaterThanOrEqual(ReservePrice);

        RaiseDomainEvent(new AuctionEndedEvent(Id, WinnerId, hasWinner.Value, CurrentPrice));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == AuctionStatus.Ended || Status == AuctionStatus.Completed)
            return Result.Failure(
                new Error("Auction.CannotCancel", "Não é possível cancelar leilão finalizado"));

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(
                new Error("Auction.InvalidReason", "Motivo do cancelamento é obrigatório"));

        Status = AuctionStatus.Cancelled;
        RaiseDomainEvent(new AuctionCancelledEvent(Id, reason));

        return Result.Success();
    }

    public Result Pause(string reason)
    {
        if (Status != AuctionStatus.Active)
            return Result.Failure(
                new Error("Auction.NotActive", "Leilão não está ativo"));

        Status = AuctionStatus.Paused;
        return Result.Success();
    }

    public Result Resume()
    {
        if (Status != AuctionStatus.Paused)
            return Result.Failure(
                new Error("Auction.NotPaused", "Leilão não está pausado"));

        Status = AuctionStatus.Active;
        return Result.Success();
    }

}
