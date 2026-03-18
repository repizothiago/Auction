namespace Auction.SharedKernel.Errors;

/// <summary>
/// Tipo de erro para facilitar o mapeamento para HTTP status codes
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Erro de falha genérica (400 Bad Request)
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Erro de validação (422 Unprocessable Entity)
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Recurso não encontrado (404 Not Found)
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// Conflito de estado ou concorrência (409 Conflict)
    /// </summary>
    Conflict = 3,

    /// <summary>
    /// Acesso negado (403 Forbidden)
    /// </summary>
    Forbidden = 4
}
