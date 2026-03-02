namespace Auction.SharedKernel.Errors;

public static class PasswordErrors
{
    public static Error Empty => new("Password.Empty", "Senha não pode ser vazia.");
    public static Error TooShort => new("Password.TooShort", "Senha deve ter no mínimo 8 caracteres.");
    public static Error MissingUppercase => new("Password.MissingUppercase", "Senha deve conter pelo menos uma letra maiúscula.");
    public static Error MissingLowercase => new("Password.MissingLowercase", "Senha deve conter pelo menos uma letra minúscula.");
    public static Error MissingDigit => new("Password.MissingDigit", "Senha deve conter pelo menos um número.");
    public static Error MissingSpecialChar => new("Password.MissingSpecialChar", "Senha deve conter pelo menos um caractere especial.");
}
