using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly AppDbContext _context;

    public AuctionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _context.Auctions
            .AsQueryable()
            .AsNoTracking()
            .Include(a => a.Category)
            .Where(a => a.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Auction>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Auctions
            .AsQueryable()
            .AsNoTracking()
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Auction>> GetActiveAuctionsAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Auctions
            .Include(a => a.Category)
            .Where(a => a.Status == AuctionStatus.Active);

        if (categoryId.HasValue)
            query = query.Where(a => a.Category.Id == categoryId.Value);

        return await query
            .OrderBy(a => a.EndDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveAuctionsCountAsync(
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Auctions
            .Where(a => a.Status == AuctionStatus.Active);

        if (categoryId.HasValue)
            query = query.Where(a => a.Category.Id == categoryId.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Auction>> GetScheduledAuctionsReadyToStartAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Auctions
            .Where(a => a.Status == AuctionStatus.Scheduled
                && a.StartDate <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Auction>> GetExpiredActiveAuctionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Auctions
            .Where(a => a.Status == AuctionStatus.Active
                && a.EndDate <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Auction>> GetBySellerIdAsync(
        Guid sellerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Auctions
            .Include(a => a.Category)
            .Where(a => a.SellerId == sellerId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.Auction auction, CancellationToken cancellationToken = default)
    {
        await _context.Auctions.AddAsync(auction, cancellationToken);
    }

    public void Update(Domain.Entities.Auction auction)
    {
        _context.Auctions.Update(auction);
    }

    public void Remove(Domain.Entities.Auction auction)
    {
        _context.Auctions.Remove(auction);
    }
}
