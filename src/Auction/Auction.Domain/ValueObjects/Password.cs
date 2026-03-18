using Auction.SharedKernel;
using Auction.SharedKernel.Errors;
using System.Security.Cryptography;
using System.Text;

namespace Auction.Domain.ValueObjects;

public sealed record Password
{
    private const int MinimumLength = 8;
    private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    public string HashedValue { get; }

    private Password(string hashedValue)
    {
        HashedValue = hashedValue;
    }

    public static Result<Password> Create(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            return Result<Password>.Failure(PasswordErrors.Empty);

        if (plainPassword.Length < MinimumLength)
            return Result<Password>.Failure(PasswordErrors.TooShort);

        if (!plainPassword.Any(char.IsUpper))
            return Result<Password>.Failure(PasswordErrors.MissingUppercase);

        if (!plainPassword.Any(char.IsLower))
            return Result<Password>.Failure(PasswordErrors.MissingLowercase);

        if (!plainPassword.Any(char.IsDigit))
            return Result<Password>.Failure(PasswordErrors.MissingDigit);

        if (!plainPassword.Any(c => SpecialCharacters.Contains(c)))
            return Result<Password>.Failure(PasswordErrors.MissingSpecialChar);

        var hashedPassword = HashPassword(plainPassword);
        return Result<Password>.Success(new Password(hashedPassword));
    }

    public bool Verify(string plainPassword)
    {
        var hashedInput = HashPassword(plainPassword);
        return HashedValue == hashedInput;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
