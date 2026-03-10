using Auction.Application.Interfaces;
using Auction.Application.Services;
using Auction.Domain.Entities.Base;

namespace Auction.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public UnitOfWork(
        AppDbContext context,
        IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capturar Domain Events antes de salvar
        var domainEvents = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Salvar mudanças no banco
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Despachar Domain Events APÓS salvar com sucesso (pattern: outbox)
        foreach (var domainEvent in domainEvents)
        {
            await _eventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        // Limpar eventos das entidades
        _context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(e => e.Entity)
            .ToList()
            .ForEach(e => e.ClearDomainEvents());

        return result;
    }

    public async Task<bool> SaveChangesAndReturnStatusAsync(CancellationToken cancellationToken = default)
    {
        var result = await SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
