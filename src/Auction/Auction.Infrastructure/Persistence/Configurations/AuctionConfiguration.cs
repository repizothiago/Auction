using Auction.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Domain.Entities.Auction>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Auction> builder)
    {
        builder.ToTable("auctions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(a => a.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // Value Object: Money - StartingPrice
        builder.OwnsOne(a => a.StartingPrice, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("starting_price_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("starting_price_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        // Value Object: Money - ReservePrice
        builder.OwnsOne(a => a.ReservePrice, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("reserve_price_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("reserve_price_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        // Value Object: Money - BuyNowPrice (Opcional)
        builder.OwnsOne(a => a.BuyNowPrice, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("buy_now_price_amount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("buy_now_price_currency")
                .HasMaxLength(3);
        });

        // Value Object: Money - CurrentPrice
        builder.OwnsOne(a => a.CurrentPrice, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("current_price_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("current_price_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        // Value Object: Money - BidIncrement
        builder.OwnsOne(a => a.BidIncrement, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("bid_increment_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("bid_increment_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        // Dates
        builder.Property(a => a.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(a => a.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        // Status (Enum como string)
        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        // Relationships (Foreign Keys)
        builder.Property(a => a.SellerId)
            .HasColumnName("seller_id")
            .IsRequired();

        builder.Property(a => a.WinnerId)
            .HasColumnName("winner_id");

        builder.Property(a => a.CurrentWinningBidId)
            .HasColumnName("current_winning_bid_id");

        // Navigation Property - Category
        builder.HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey("category_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Value Object: AuctionRules (como JSON)
        builder.OwnsOne(a => a.Rules, rules =>
        {
            rules.ToJson("rules");

            rules.Property(r => r.ExtensionTime)
                .HasColumnName("extension_time");

            rules.Property(r => r.ExtensionWindow)
                .HasColumnName("extension_window");

            rules.Property(r => r.MaxBidsPerUser)
                .HasColumnName("max_bids_per_user");

            rules.Property(r => r.AllowProxyBids)
                .HasColumnName("allow_proxy_bids");
        });

        // Stats
        builder.Property(a => a.TotalBids)
            .HasColumnName("total_bids")
            .HasDefaultValue(0)
            .IsRequired();

        // Audit fields (BaseEntity)
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(a => a.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        // Índices para performance
        builder.HasIndex(a => a.Status)
            .HasDatabaseName("idx_auctions_status");

        builder.HasIndex(a => a.EndDate)
            .HasDatabaseName("idx_auctions_end_date");

        builder.HasIndex(a => a.SellerId)
            .HasDatabaseName("idx_auctions_seller");

        builder.HasIndex(new[] { "category_id" })
            .HasDatabaseName("idx_auctions_category");

        // Índice composto para leilões ativos próximos do fim
        builder.HasIndex(a => new { a.EndDate, a.Status })
            .HasDatabaseName("idx_auctions_active_ending")
            .HasFilter("status = 'Active'");

        // Ignorar Domain Events (não persiste no banco)
        builder.Ignore(a => a.DomainEvents);
    }
}
