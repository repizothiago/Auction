using Auction.SharedKernel;
using Auction.SharedKernel.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Extensions;

/// <summary>
/// Métodos de extensão para converter Result em ProblemDetails (camada de apresentação)
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converte um Result com erro para ProblemDetails
    /// </summary>
    public static ProblemDetails ToProblemDetails(this Result result)
    {
        if (result.IsSuccess || result.Error is null)
        {
            throw new InvalidOperationException("Cannot convert successful result to ProblemDetails");
        }

        return new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Message,
            Status = result.Error.Type.ToHttpStatusCode()
        };
    }

    /// <summary>
    /// Converte um Result&lt;T&gt; com erro para ProblemDetails
    /// </summary>
    public static ProblemDetails ToProblemDetails<T>(this Result<T> result)
    {
        if (result.IsSuccess || result.Error is null)
        {
            throw new InvalidOperationException("Cannot convert successful result to ProblemDetails");
        }

        return new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Message,
            Status = result.Error.Type.ToHttpStatusCode()
        };
    }

    /// <summary>
    /// Mapeia ErrorType para HTTP status code (lógica de apresentação)
    /// </summary>
    private static int ToHttpStatusCode(this ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
