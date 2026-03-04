using Auction.Application.CommandHandlers.Auction;
using Auction.Application.Commands;
using Auction.Application.Commands.Auction;
using Auction.Application.EventHandlers;
using Auction.Application.Services;
using Auction.Domain.Events.Auction;
using Auction.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Auction.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Domain Event Handlers (quando domain event é disparado, publica no Kafka)
        services.AddScoped<IDomainEventHandler<AuctionCancelledEvent>, AuctionCancelledEventHandler>();

        // Registrar outros domain event handlers conforme necessário:
        // services.AddScoped<IDomainEventHandler<AuctionCreatedEvent>, AuctionCreatedEventHandler>();
        // services.AddScoped<IDomainEventHandler<AuctionStartedEvent>, AuctionStartedEventHandler>();
        // services.AddScoped<IDomainEventHandler<AuctionEndedEvent>, AuctionEndedEventHandler>();
        // services.AddScoped<IDomainEventHandler<BidPlacedEvent>, BidPlacedEventHandler>();

        // Command Handlers (CQRS)
        services.AddScoped<ICommandHandler<CancelAuctionCommand>, CancelAuctionCommandHandler>();

        // Registrar outros command handlers conforme necessário:
        // services.AddScoped<ICommandHandler<CreateAuctionCommand, Guid>, CreateAuctionCommandHandler>();
        // services.AddScoped<ICommandHandler<PlaceBidCommand>, PlaceBidCommandHandler>();
        // services.AddScoped<ICommandHandler<StartAuctionCommand>, StartAuctionCommandHandler>();

        return services;
    }
}
