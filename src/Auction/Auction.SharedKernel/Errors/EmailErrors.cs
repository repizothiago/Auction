namespace Auction.SharedKernel.Errors;

public static class EmailErrors
{
    public static Error Empty => Error.Validation("Email.Empty", "E-mail não pode ser vazio.");
    public static Error Invalid => Error.Validation("Email.Invalid", "E-mail inválido.");
}
