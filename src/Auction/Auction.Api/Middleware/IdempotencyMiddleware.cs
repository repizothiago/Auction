using Auction.Application.Interfaces;

namespace Auction.Api.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(
        RequestDelegate next,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        // Apenas processar POST/PUT/PATCH
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Verificar se há header de idempotência
        if (!context.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        var key = $"idempotency:{idempotencyKey}";

        // Verificar se já existe resposta em cache
        var cachedResponse = await cacheService.GetAsync<CachedIdempotentResponse>(key);
        if (cachedResponse is not null)
        {
            _logger.LogInformation(
                "Requisição duplicada detectada. IdempotencyKey={IdempotencyKey}", 
                idempotencyKey.ToString());

            // Retornar resposta em cache
            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = "application/json";

            foreach (var header in cachedResponse.Headers)
            {
                context.Response.Headers[header.Key] = header.Value;
            }

            await context.Response.WriteAsync(cachedResponse.Body);
            return;
        }

        // Capturar resposta original
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Se requisição foi bem-sucedida (2xx), cachear resposta
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

                var cachedData = new CachedIdempotentResponse
                {
                    StatusCode = context.Response.StatusCode,
                    Body = responseBodyText,
                    Headers = context.Response.Headers
                        .Where(h => !h.Key.StartsWith("Transfer-", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(h => h.Key, h => h.Value.ToString())
                };

                // Cachear por 24 horas
                await cacheService.SetAsync(key, cachedData, TimeSpan.FromHours(24));

                _logger.LogInformation(
                    "Resposta cacheada para idempotência. IdempotencyKey={IdempotencyKey}",
                    idempotencyKey.ToString());
            }

            // Copiar resposta de volta para o stream original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

public class CachedIdempotentResponse
{
    public int StatusCode { get; set; }
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}
