using Auction.Application.CommandHandlers.Auction;
using Auction.Application.CommandHandlers.Bid;
using Auction.Application.Commands;
using Auction.Application.Commands.Auction;
using Auction.Application.Commands.Bid;
using Auction.Application.EventHandlers;
using Auction.Application.EventHandlers.Bid;
using Auction.Application.Queries.Auction;
using Auction.Application.Services;
using Auction.Domain.Events.Auction;
using Auction.Domain.Events.Bid;
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
        services.AddScoped<IDomainEventHandler<BidPlacedEvent>, BidPlacedEventHandler>();
        services.AddScoped<IDomainEventHandler<BidOutbidEvent>, BidOutbidEventHandler>();

        // Registrar outros domain event handlers conforme necessário:
        // services.AddScoped<IDomainEventHandler<AuctionCreatedEvent>, AuctionCreatedEventHandler>();
        // services.AddScoped<IDomainEventHandler<AuctionStartedEvent>, AuctionStartedEventHandler>();
        // services.AddScoped<IDomainEventHandler<AuctionEndedEvent>, AuctionEndedEventHandler>();

        // Command Handlers (CQRS)
        services.AddScoped<ICommandHandler<CancelAuctionCommand>, CancelAuctionCommandHandler>();
        services.AddScoped<PlaceBidCommandHandler>();

        // Registrar outros command handlers conforme necessário:
        // services.AddScoped<ICommandHandler<CreateAuctionCommand, Guid>, CreateAuctionCommandHandler>();
        // services.AddScoped<ICommandHandler<StartAuctionCommand>, StartAuctionCommandHandler>();

        // Query Handlers (CQRS)
        services.AddScoped<GetAllAuctionsQueryHandler>();

        return services;
    }
}
