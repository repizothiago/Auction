using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.ValueObjects;

public sealed record Cnpj
{
    public string Value { get; }

    private Cnpj(string value)
    {
        Value = value;
    }

    public static Result<Cnpj> Create(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return Result<Cnpj>.Failure(new Error("Cnpj.Empty", "CNPJ não pode ser vazio."));

        var cleanCnpj = new string(cnpj.Where(char.IsDigit).ToArray());

        if (cleanCnpj.Length != 14)
            return Result<Cnpj>.Failure(new Error("Cnpj.InvalidLength", "CNPJ deve ter 14 dígitos."));

        if (!IsValid(cleanCnpj))
            return Result<Cnpj>.Failure(new Error("Cnpj.Invalid", "CNPJ inválido."));

        return Result<Cnpj>.Success(new Cnpj(cleanCnpj));
    }

    private static bool IsValid(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1)
            return false;

        int[] firstMultiplier = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] secondMultiplier = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCnpj = cnpj.Substring(0, 12);
        int sum = 0;

        for (int i = 0; i < 12; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * firstMultiplier[i];

        int remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;

        string digit = remainder.ToString();
        tempCnpj += digit;
        sum = 0;

        for (int i = 0; i < 13; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * secondMultiplier[i];

        remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        digit += remainder.ToString();

        return cnpj.EndsWith(digit);
    }
}