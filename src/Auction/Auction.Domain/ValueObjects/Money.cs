using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.ValueObjects;

public sealed record Money
{
    public decimal Value { get; }
    public string Currency { get; }


    private Money(decimal value, string currency)
    {
        Value = value;
        Currency = currency;
    }
    public bool IsDistinctCurrency(Money other) => Currency != other.Currency;

    public Result<bool> IsGreaterThanOrEqual(Money other)
    {
        if (IsDistinctCurrency(other))
            return Result<bool>.Failure(new Error("Money.DistinctCurrency", "Cannot compare amounts with different currencies."));

        return Result<bool>.Success(Value >= other.Value);
    }

    public Money Add(Money other)
    {
        if (IsDistinctCurrency(other))
            throw new InvalidOperationException("Cannot add amounts with different currencies.");

        return new Money(Value + other.Value, Currency);
    }


    public static Result<Money> Create(decimal value, string currency)
    {
        if (value < 0)
            return Result<Money>.Failure(new Error("Money.InvalidValue", "O valor do dinheiro não pode ser negativo."));
        if (string.IsNullOrWhiteSpace(currency))
            return Result<Money>.Failure(new Error("Money.InvalidCurrency", "A moeda é obrigatória."));
        return Result<Money>.Success(new Money(value, currency));
    }
}
