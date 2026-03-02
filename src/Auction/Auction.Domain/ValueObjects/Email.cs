using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.ValueObjects;

public sealed partial record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure(EmailErrors.Empty);

        var normalizedEmail = Normalize(email);

        return Result<Email>.Success(new Email(normalizedEmail));
    }

    private static string Normalize(string email)
    {
        return email.Trim().ToLower();
    }
}
