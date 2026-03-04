namespace Auction.Application.Services;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(Auction.SharedKernel.IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
