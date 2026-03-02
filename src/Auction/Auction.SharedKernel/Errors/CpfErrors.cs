namespace Auction.SharedKernel.Errors;

public static class CpfErrors
{
    public static Error Empty => new("Cpf.Empty", "CPF não pode ser vazio.");
    public static Error Invalid => new("Cpf.Invalid", "CPF inválido.");
}
