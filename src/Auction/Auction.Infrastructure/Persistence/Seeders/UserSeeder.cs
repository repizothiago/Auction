using Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence.Seeders;

public static class UserSeeder
{
    public static void SeedUsers(ModelBuilder modelBuilder)
    {
        var baseDate = DateTime.UtcNow;

        // Seed Individual Users (Pessoas Físicas)
        var individual1 = new
        {
            Id = new Guid("10000000-0000-0000-0000-000000000001"),
            user_type = "Individual",
            CreatedAt = baseDate.AddMonths(-3),
            UpdatedAt = (DateTime?)null,
            Version = 0L
        };

        var individual2 = new
        {
            Id = new Guid("10000000-0000-0000-0000-000000000002"),
            user_type = "Individual",
            CreatedAt = baseDate.AddMonths(-3),
            UpdatedAt = (DateTime?)null,
            Version = 0L
        };

        var individual3 = new
        {
            Id = new Guid("10000000-0000-0000-0000-000000000003"),
            user_type = "Individual",
            CreatedAt = baseDate.AddMonths(-3),
            UpdatedAt = (DateTime?)null,
            Version = 0L
        };

        var individual4 = new
        {
            Id = new Guid("10000000-0000-0000-0000-000000000004"),
            user_type = "Individual",
            CreatedAt = baseDate.AddMonths(-3),
            UpdatedAt = (DateTime?)null,
            Version = 0L
        };

        // Seed Corporate Users (Pessoas Jurídicas)
        var corporate1 = new
        {
            Id = new Guid("20000000-0000-0000-0000-000000000001"),
            user_type = "Corporate",
            CompanyName = "Tech Leilões LTDA",
            CreatedAt = baseDate.AddMonths(-2),
            UpdatedAt = (DateTime?)null,
            Version = 0L
        };

        var corporate2 = new
        {
            Id = new Guid("20000000-0000-0000-0000-000000000002"),
            user_type = "Corporate",
            CompanyName = "Arte Galeria S.A.",
            CreatedAt = baseDate.AddMonths(-2),
            UpdatedAt = (DateTime?)null,
            Version = 0L
        };

        var corporate3 = new
        {
            Id = new Guid("20000000-0000-0000-0000-000000000003"),
            user_type = "Corporate",
            CompanyName = "Veículos Premium LTDA",
            CreatedAt = baseDate.AddMonths(-2),
            UpdatedAt = (DateTime?)null,
            Version = 0L
        };

        modelBuilder.Entity<IndividualEntity>().HasData(
            individual1, individual2, individual3, individual4
        );

        modelBuilder.Entity<CorporateEntity>().HasData(
            corporate1, corporate2, corporate3
        );

        // Seed Email Value Objects
        modelBuilder.Entity<IndividualEntity>()
            .OwnsOne(u => u.Email)
            .HasData(
                new { UserId = new Guid("10000000-0000-0000-0000-000000000001"), Value = "joao.silva@email.com" },
                new { UserId = new Guid("10000000-0000-0000-0000-000000000002"), Value = "maria.santos@email.com" },
                new { UserId = new Guid("10000000-0000-0000-0000-000000000003"), Value = "pedro.oliveira@email.com" },
                new { UserId = new Guid("10000000-0000-0000-0000-000000000004"), Value = "ana.costa@email.com" }
            );

        modelBuilder.Entity<CorporateEntity>()
            .OwnsOne(u => u.Email)
            .HasData(
                new { UserId = new Guid("20000000-0000-0000-0000-000000000001"), Value = "contato@techleiloes.com.br" },
                new { UserId = new Guid("20000000-0000-0000-0000-000000000002"), Value = "vendas@artegaleria.com.br" },
                new { UserId = new Guid("20000000-0000-0000-0000-000000000003"), Value = "contato@veiculospremium.com.br" }
            );

        // Seed Password Value Objects
        const string passwordHash = "$2a$11$N9qo8uLOickgx2ZMRZoMye"; // hash de "Password123"

        modelBuilder.Entity<IndividualEntity>()
            .OwnsOne(u => u.Password)
            .HasData(
                new { UserId = new Guid("10000000-0000-0000-0000-000000000001"), HashedValue = passwordHash },
                new { UserId = new Guid("10000000-0000-0000-0000-000000000002"), HashedValue = passwordHash },
                new { UserId = new Guid("10000000-0000-0000-0000-000000000003"), HashedValue = passwordHash },
                new { UserId = new Guid("10000000-0000-0000-0000-000000000004"), HashedValue = passwordHash }
            );

        modelBuilder.Entity<CorporateEntity>()
            .OwnsOne(u => u.Password)
            .HasData(
                new { UserId = new Guid("20000000-0000-0000-0000-000000000001"), HashedValue = passwordHash },
                new { UserId = new Guid("20000000-0000-0000-0000-000000000002"), HashedValue = passwordHash },
                new { UserId = new Guid("20000000-0000-0000-0000-000000000003"), HashedValue = passwordHash }
            );

        // Seed CPF Value Objects (Individual)
        modelBuilder.Entity<IndividualEntity>()
            .OwnsOne(i => i.Cpf)
            .HasData(
                new { IndividualEntityId = new Guid("10000000-0000-0000-0000-000000000001"), Value = "12345678901" },
                new { IndividualEntityId = new Guid("10000000-0000-0000-0000-000000000002"), Value = "23456789012" },
                new { IndividualEntityId = new Guid("10000000-0000-0000-0000-000000000003"), Value = "34567890123" },
                new { IndividualEntityId = new Guid("10000000-0000-0000-0000-000000000004"), Value = "45678901234" }
            );

        // Seed CNPJ Value Objects (Corporate)
        modelBuilder.Entity<CorporateEntity>()
            .OwnsOne(c => c.Cnpj)
            .HasData(
                new { CorporateEntityId = new Guid("20000000-0000-0000-0000-000000000001"), Value = "12345678000190" },
                new { CorporateEntityId = new Guid("20000000-0000-0000-0000-000000000002"), Value = "98765432000101" },
                new { CorporateEntityId = new Guid("20000000-0000-0000-0000-000000000003"), Value = "11122233000144" }
            );
    }
}
