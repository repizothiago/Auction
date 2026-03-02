using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.ValueObjects;

public sealed record Cpf
{
    public string Value { get; }

    private Cpf(string value)
    {
        Value = value;
    }

    public static Result<Cpf> Create(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return Result<Cpf>.Failure(CpfErrors.Empty);

        var cleanCpf = cpf.Replace(".", "").Replace("-", "").Trim();

        if (!IsValid(cleanCpf))
            return Result<Cpf>.Failure(CpfErrors.Invalid);

        return Result<Cpf>.Success(new Cpf(cleanCpf));
    }

    private static bool IsValid(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        var numbers = cpf.Select(c => c - '0').ToArray();

        var sum1 = 0;
        for (int i = 0; i < 9; i++)
            sum1 += numbers[i] * (10 - i);

        var remainder1 = sum1 % 11;
        var digit1 = remainder1 < 2 ? 0 : 11 - remainder1;

        if (numbers[9] != digit1)
            return false;

        var sum2 = 0;
        for (int i = 0; i < 10; i++)
            sum2 += numbers[i] * (11 - i);

        var remainder2 = sum2 % 11;
        var digit2 = remainder2 < 2 ? 0 : 11 - remainder2;

        return numbers[10] == digit2;
    }

    public override string ToString()
        => Convert.ToUInt64(Value).ToString(@"000\.000\.000\-00");
}
