namespace Auction.SharedKernel.Errors;

/// <summary>
/// Representa um erro de domínio ou aplicação
/// </summary>
public class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    public Error(string code, string message, ErrorType type = ErrorType.Failure)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    /// <summary>
    /// Cria um erro de validação (422)
    /// </summary>
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    /// <summary>
    /// Cria um erro de não encontrado (404)
    /// </summary>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Cria um erro de conflito (409)
    /// </summary>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Cria um erro de acesso negado (403)
    /// </summary>
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    /// <summary>
    /// Cria um erro de falha genérica (400)
    /// </summary>
    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);
}
