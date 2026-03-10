using Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("bids");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(b => b.AuctionId)
            .HasColumnName("auction_id")
            .IsRequired();

        builder.Property(b => b.BidderId)
            .HasColumnName("bidder_id")
            .IsRequired();

        builder.Property(b => b.IsAutoBid)
            .HasColumnName("is_auto_bid")
            .IsRequired();

        builder.Property(b => b.BidStatus)
            .HasColumnName("bid_status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(b => b.BidTime)
            .HasColumnName("bid_time")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(b => b.Version)
            .HasColumnName("version")
            .IsRowVersion()
            .IsRequired();

        // Configurar Value Object Money como Owned Entity
        builder.OwnsOne(b => b.Amount, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Relacionamento com Auction
        builder.HasOne<Domain.Entities.Auction>()
            .WithMany()
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento com User (Bidder)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes para performance
        builder.HasIndex(b => b.AuctionId)
            .HasDatabaseName("ix_bids_auction_id");

        builder.HasIndex(b => b.BidderId)
            .HasDatabaseName("ix_bids_bidder_id");

        builder.HasIndex(b => b.BidTime)
            .HasDatabaseName("ix_bids_bid_time");

        builder.HasIndex(b => new { b.AuctionId, b.BidStatus })
            .HasDatabaseName("ix_bids_auction_status");

        // Ignore domain events (não persiste no banco)
        builder.Ignore(b => b.DomainEvents);
    }
}
