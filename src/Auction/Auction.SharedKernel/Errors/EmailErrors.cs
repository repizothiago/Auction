namespace Auction.SharedKernel.Errors;

public static class EmailErrors
{
    public static Error Empty => new("Email.Empty", "E-mail não pode ser vazio.");
    public static Error Invalid => new("Email.Invalid", "E-mail inválido.");
}
