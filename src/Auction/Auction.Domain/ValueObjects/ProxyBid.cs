using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.ValueObjects;

public sealed record ProxyBid
{
    public ProxyBid(Money maxAmount, Money currentAmount, bool isActive)
    {
        MaxAmount = maxAmount;
        CurrentAmount = currentAmount;
        IsActive = isActive;
    }

    public Money MaxAmount { get; }
    public Money CurrentAmount { get; }
    public bool IsActive { get; private set; }

    public Result<Money> TryIncreaseBid(Money competitorBid, Money increment)
    {
        if (!IsActive)
            return Result<Money>.Failure(new Error("ProxyBid.Inactive", "Lance proxy inativo."));

        var newAmount = competitorBid.Value + increment.Value;

        if (newAmount > MaxAmount.Value)
        {
            IsActive = false; // Atingiu o limite
            return Result<Money>.Failure(new Error("ProxyBid.MaxReached", "Valor máximo atingido."));
        }

        var newValue = Money.Create(newAmount, MaxAmount.Currency);

        return Result<Money>.Success(newValue.Value);
    }
}
