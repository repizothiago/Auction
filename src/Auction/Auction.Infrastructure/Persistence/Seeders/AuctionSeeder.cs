using Auction.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence.Seeders;

public static class AuctionSeeder
{
    // IDs das categorias já existentes
    private static readonly Guid EletronicosId = new("1563318e-7548-4620-a936-9d0ed752f2bc");
    private static readonly Guid VeiculosId = new("28477285-2e12-43a6-8270-de0bf9519d20");
    private static readonly Guid ArteId = new("984bc72b-4c10-4826-8024-6c478f0c3c60");
    private static readonly Guid JoiasId = new("71b730fb-7549-4cb2-bdb2-091b0e7652bd");
    private static readonly Guid ImoveisId = new("7f7fa068-16c8-43e0-b2c5-20e50311d474");

    // IDs dos usuários (sellers)
    private static readonly Guid Corporate1Id = new("20000000-0000-0000-0000-000000000001");
    private static readonly Guid Corporate2Id = new("20000000-0000-0000-0000-000000000002");
    private static readonly Guid Corporate3Id = new("20000000-0000-0000-0000-000000000003");
    private static readonly Guid Individual1Id = new("10000000-0000-0000-0000-000000000001");

    public static void SeedAuctions(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;

        // JSON padrão para AuctionRules
        const string defaultRules = @"{""ExtensionTime"":""00:05:00"",""ExtensionWindow"":""00:05:00"",""MaxBidsPerUser"":10,""AllowProxyBids"":true}";

        // Leilão 1: Laptop Dell - Ativo
        var auction1Id = new Guid("30000000-0000-0000-0000-000000000001");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction1Id,
            Title = "Laptop Dell XPS 15 - i7 16GB RAM",
            Description = "Notebook Dell XPS 15 polegadas, processador Intel i7 11ª geração, 16GB RAM, SSD 512GB, placa de vídeo dedicada GTX 1650. Estado de conservação excelente.",
            StartDate = now.AddDays(-2),
            EndDate = now.AddDays(5),
            Status = AuctionStatus.Active,
            SellerId = Corporate1Id,
            WinnerId = (Guid?)null,
            TotalBids = 15,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddMonths(-1),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = EletronicosId
        });

        // Value Objects para Leilão 1
        SeedAuctionMoney(modelBuilder, auction1Id, 2500.00m, 2500.00m, 3000.00m, 2500.00m, 50.00m);

        // Leilão 2: iPhone 14 Pro - Ativo
        var auction2Id = new Guid("30000000-0000-0000-0000-000000000002");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction2Id,
            Title = "iPhone 14 Pro Max 256GB - Lacrado",
            Description = "iPhone 14 Pro Max 256GB, cor Space Black, lacrado na caixa. Garantia Apple de 1 ano. Acompanha todos os acessórios originais.",
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(3),
            Status = AuctionStatus.Active,
            SellerId = Corporate1Id,
            WinnerId = (Guid?)null,
            TotalBids = 8,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddMonths(-1),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = EletronicosId
        });

        SeedAuctionMoney(modelBuilder, auction2Id, 4500.00m, 4500.00m, 5500.00m, 4800.00m, 100.00m);

        // Leilão 3: Honda Civic 2020 - Ativo
        var auction3Id = new Guid("30000000-0000-0000-0000-000000000003");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction3Id,
            Title = "Honda Civic Touring 2020 - Apenas 30.000 km",
            Description = "Honda Civic Touring 2.0 16V Flex Aut. 2020, cor prata, 30.000 km rodados, único dono, manual e chave reserva, revisões em concessionária. IPVA 2024 pago.",
            StartDate = now.AddDays(-3),
            EndDate = now.AddDays(7),
            Status = AuctionStatus.Active,
            SellerId = Corporate3Id,
            WinnerId = (Guid?)null,
            TotalBids = 22,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddMonths(-2),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = VeiculosId
        });

        SeedAuctionMoney(modelBuilder, auction3Id, 85000.00m, 85000.00m, 95000.00m, 87500.00m, 500.00m);

        // Leilão 4: Quadro Pintura - Agendado
        var auction4Id = new Guid("30000000-0000-0000-0000-000000000004");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction4Id,
            Title = "Pintura a Óleo 'Pôr do Sol' - Artista Renomado",
            Description = "Obra de arte original, pintura a óleo sobre tela 100x80cm, assinada pelo artista brasileiro João Mendes. Acompanha certificado de autenticidade.",
            StartDate = now.AddDays(2),
            EndDate = now.AddDays(10),
            Status = AuctionStatus.Scheduled,
            SellerId = Corporate2Id,
            WinnerId = (Guid?)null,
            TotalBids = 0,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddMonths(-1),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = ArteId
        });

        SeedAuctionMoney(modelBuilder, auction4Id, 15000.00m, 15000.00m, 20000.00m, 15000.00m, 500.00m);

        // Leilão 5: Colar de Diamantes - Agendado
        var auction5Id = new Guid("30000000-0000-0000-0000-000000000005");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction5Id,
            Title = "Colar de Diamantes 18K - 2 Quilates",
            Description = "Colar em ouro 18K com diamantes naturais totalizando 2 quilates. Design exclusivo, acompanha certificado gemológico e estojo de luxo.",
            StartDate = now.AddDays(1),
            EndDate = now.AddDays(8),
            Status = AuctionStatus.Scheduled,
            SellerId = Corporate2Id,
            WinnerId = (Guid?)null,
            TotalBids = 0,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddDays(-14),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = JoiasId
        });

        SeedAuctionMoney(modelBuilder, auction5Id, 35000.00m, 35000.00m, 45000.00m, 35000.00m, 1000.00m);

        // Leilão 6: Apartamento SP - Agendado
        var auction6Id = new Guid("30000000-0000-0000-0000-000000000006");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction6Id,
            Title = "Apartamento 3 Quartos - Jardins, São Paulo/SP",
            Description = "Apartamento 120m², 3 quartos sendo 1 suíte, 2 vagas de garagem, sacada gourmet. Edifício com piscina, academia e salão de festas. Localização privilegiada nos Jardins.",
            StartDate = now.AddDays(5),
            EndDate = now.AddDays(15),
            Status = AuctionStatus.Scheduled,
            SellerId = Individual1Id,
            WinnerId = (Guid?)null,
            TotalBids = 0,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddDays(-7),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = ImoveisId
        });

        SeedAuctionMoney(modelBuilder, auction6Id, 850000.00m, 850000.00m, 950000.00m, 850000.00m, 5000.00m);

        // Leilão 7: Smart TV Samsung - Finalizado
        var auction7Id = new Guid("30000000-0000-0000-0000-000000000007");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction7Id,
            Title = "Smart TV Samsung 65'' QLED 4K",
            Description = "Smart TV Samsung 65 polegadas, tecnologia QLED, resolução 4K, HDR, 120Hz. Nota fiscal e garantia de 1 ano. Perfeito estado.",
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(-3),
            Status = AuctionStatus.Ended,
            SellerId = Corporate1Id,
            WinnerId = new Guid("10000000-0000-0000-0000-000000000002"), // Maria Santos
            TotalBids = 28,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddMonths(-3),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = EletronicosId
        });

        SeedAuctionMoney(modelBuilder, auction7Id, 3500.00m, 3500.00m, 4500.00m, 4200.00m, 100.00m);

        // Leilão 8: PlayStation 5 - Finalizado
        var auction8Id = new Guid("30000000-0000-0000-0000-000000000008");
        modelBuilder.Entity<Domain.Entities.Auction>().HasData(new
        {
            Id = auction8Id,
            Title = "PlayStation 5 + 2 Controles + 5 Jogos",
            Description = "Console PlayStation 5 versão com leitor de disco, 2 controles DualSense, 5 jogos físicos (God of War, Spider-Man, Horizon, FIFA 24, GT7). Garantia até 2025.",
            StartDate = now.AddDays(-15),
            EndDate = now.AddDays(-5),
            Status = AuctionStatus.Ended,
            SellerId = Corporate1Id,
            WinnerId = new Guid("10000000-0000-0000-0000-000000000003"), // Pedro Oliveira
            TotalBids = 35,
            CurrentWinningBidId = (Guid?)null,
            CreatedAt = now.AddMonths(-3),
            UpdatedAt = (DateTime?)null,
            Version = 0L,
            category_id = EletronicosId
        });

        SeedAuctionMoney(modelBuilder, auction8Id, 2800.00m, 2800.00m, 3500.00m, 3400.00m, 50.00m);
    }

    private static void SeedAuctionMoney(
        ModelBuilder modelBuilder,
        Guid auctionId,
        decimal startingPrice,
        decimal reservePrice,
        decimal? buyNowPrice,
        decimal currentPrice,
        decimal bidIncrement)
    {
        // StartingPrice
        modelBuilder.Entity<Domain.Entities.Auction>()
            .OwnsOne(a => a.StartingPrice)
            .HasData(new { AuctionId = auctionId, Value = startingPrice, Currency = "BRL" });

        // ReservePrice
        modelBuilder.Entity<Domain.Entities.Auction>()
            .OwnsOne(a => a.ReservePrice)
            .HasData(new { AuctionId = auctionId, Value = reservePrice, Currency = "BRL" });

        // BuyNowPrice (opcional)
        if (buyNowPrice.HasValue)
        {
            modelBuilder.Entity<Domain.Entities.Auction>()
                .OwnsOne(a => a.BuyNowPrice)
                .HasData(new { AuctionId = auctionId, Value = buyNowPrice.Value, Currency = "BRL" });
        }

        // CurrentPrice
        modelBuilder.Entity<Domain.Entities.Auction>()
            .OwnsOne(a => a.CurrentPrice)
            .HasData(new { AuctionId = auctionId, Value = currentPrice, Currency = "BRL" });

        // BidIncrement
        modelBuilder.Entity<Domain.Entities.Auction>()
            .OwnsOne(a => a.BidIncrement)
            .HasData(new { AuctionId = auctionId, Value = bidIncrement, Currency = "BRL" });
    }

    }
