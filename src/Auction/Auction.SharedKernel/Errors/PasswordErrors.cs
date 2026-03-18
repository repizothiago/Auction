namespace Auction.SharedKernel.Errors;

public static class PasswordErrors
{
    public static Error Empty => Error.Validation("Password.Empty", "Senha não pode ser vazia.");
    public static Error TooShort => Error.Validation("Password.TooShort", "Senha deve ter no mínimo 8 caracteres.");
    public static Error MissingUppercase => Error.Validation("Password.MissingUppercase", "Senha deve conter pelo menos uma letra maiúscula.");
    public static Error MissingLowercase => Error.Validation("Password.MissingLowercase", "Senha deve conter pelo menos uma letra minúscula.");
    public static Error MissingDigit => Error.Validation("Password.MissingDigit", "Senha deve conter pelo menos um número.");
    public static Error MissingSpecialChar => Error.Validation("Password.MissingSpecialChar", "Senha deve conter pelo menos um caractere especial.");
}
