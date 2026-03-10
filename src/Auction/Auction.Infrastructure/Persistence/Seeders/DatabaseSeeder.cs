using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder principal para popular o banco com dados de exemplo
/// </summary>
public static class DatabaseSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed apenas dos leilões (categorias e usuários já foram criados na InitialCreate)
        AuctionSeeder.SeedAuctions(modelBuilder);
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Category>().HasData(
            new
            {
                Id = new Guid("1563318e-7548-4620-a936-9d0ed752f2bc"),
                Name = "Eletrônicos",
                Description = "Dispositivos eletrônicos e gadgets",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("28477285-2e12-43a6-8270-de0bf9519d20"),
                Name = "Veículos",
                Description = "Carros, motos e outros veículos",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("2a0e3fab-1ef8-4b95-8e4a-c10277e159e7"),
                Name = "Antiguidades",
                Description = "Itens antigos e vintage",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("71b730fb-7549-4cb2-bdb2-091b0e7652bd"),
                Name = "Joias",
                Description = "Joias e pedras preciosas",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("75bbd8cf-c16c-4428-a9ca-e39b732b5a84"),
                Name = "Outros",
                Description = "Outras categorias",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("7f7fa068-16c8-43e0-b2c5-20e50311d474"),
                Name = "Imóveis",
                Description = "Casas, apartamentos e terrenos",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("984bc72b-4c10-4826-8024-6c478f0c3c60"),
                Name = "Arte",
                Description = "Obras de arte e colecionáveis",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                Version = 0L
            }
        );
    }
}

