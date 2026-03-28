namespace Auction.Infrastructure.Messaging;

/// <summary>
/// Contexto de correlação para rastreamento entre threads e tarefas assíncronas.
/// Usado pelos consumers Kafka para propagar o CorrelationId fora do contexto HTTP.
/// </summary>
public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();

    public static string? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }

    public static string GetOrCreate()
        => _current.Value ??= Guid.NewGuid().ToString();
}
