namespace Auction.SharedKernel;

/// <summary>
/// Interface para handlers de Domain Events
/// </summary>
public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
