namespace Auction.SharedKernel.Errors;

public static class CpfErrors
{
    public static Error Empty => Error.Validation("Cpf.Empty", "CPF não pode ser vazio.");
    public static Error Invalid => Error.Validation("Cpf.Invalid", "CPF inválido.");
}
