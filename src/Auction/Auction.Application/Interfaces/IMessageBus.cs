using Auction.SharedKernel;

namespace Auction.Application.Interfaces;

/// <summary>
/// Interface para publicação de eventos em Message Bus (Kafka)
/// </summary>
public interface IMessageBus
{
    Task PublishAsync<TEvent>(
        string topic,
        TEvent @event,
        string partitionKey,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent;

    Task PublishBatchAsync<TEvent>(
        string topic,
        IEnumerable<TEvent> events,
        Func<TEvent, string> partitionKeySelector,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}
