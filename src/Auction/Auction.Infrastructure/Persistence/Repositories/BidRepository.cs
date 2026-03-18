using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence.Repositories;

public class BidRepository : IBidRepository
{
    private readonly AppDbContext _context;

    public BidRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Bid?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bids
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<List<Bid>> GetByAuctionIdAsync(
        Guid auctionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bids
            .AsNoTracking()
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Bid>> GetByBidderIdAsync(
        Guid bidderId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bids
            .AsNoTracking()
            .Where(b => b.BidderId == bidderId)
            .OrderByDescending(b => b.BidTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Bid?> GetHighestBidForAuctionAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bids
            .AsNoTracking()
            .Where(b => b.AuctionId == auctionId && b.BidStatus == BidStatus.Active)
            .OrderByDescending(b => b.Amount.Value)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetBidCountForUserInAuctionAsync(
        Guid auctionId,
        Guid bidderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bids
            .AsNoTracking()
            .CountAsync(b => b.AuctionId == auctionId && b.BidderId == bidderId, cancellationToken);
    }

    public async Task AddAsync(Bid bid, CancellationToken cancellationToken = default)
    {
        await _context.Bids.AddAsync(bid, cancellationToken);
    }

    public void Update(Bid bid)
    {
        _context.Bids.Update(bid);
    }
}
