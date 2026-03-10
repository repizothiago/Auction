using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder simplificado para popular o banco com dados de exemplo via SQL
/// </summary>
public static class DatabaseSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categorias
        SeedCategories(modelBuilder);

        // Seed Users e Auctions serão feitos via migration SQL
        // (devido a Value Objects e complexidade dos relacionamentos)
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Category>().HasData(
            new
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "Eletrônicos",
                Description = "Dispositivos eletrônicos e tecnologia",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "Veículos",
                Description = "Carros, motos e veículos em geral",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "Arte",
                Description = "Obras de arte, pinturas e esculturas",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Name = "Colecionáveis",
                Description = "Itens colecionáveis e antiguidades",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Name = "Imóveis",
                Description = "Casas, apartamentos e terrenos",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null,
                Version = 0L
            },
            new
            {
                Id = new Guid("66666666-6666-6666-6666-666666666666"),
                Name = "Moda",
                Description = "Roupas, acessórios e joias",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null,
                Version = 0L
            }
        );
    }

    /// <summary>
    /// Retorna SQL completo para seed de usuários e auctions (com Rules como JSON)
    /// </summary>
    public static string GetCompleteSeedSql()
    {
        var now = DateTime.UtcNow;

        // JSON para AuctionRules - todos os leilões usam as mesmas regras
        const string rulesJson = @"{""ExtensionTime"":""00:05:00"",""ExtensionWindow"":""00:05:00"",""MaxBidsPerUser"":5,""AllowProxyBids"":true}";

        return $@"
-- Individual Users (TPH: user_type = 'Individual')
INSERT INTO users (id, user_type, email, password_hash, cpf, created_at, updated_at, version)
VALUES 
('a0000000-0000-0000-0000-000000000001', 'Individual', 'joao.silva@email.com', 
 '$2a$11$abcdefgh', '12345678901', '{now.AddMonths(-3):yyyy-MM-dd HH:mm:ss}', NULL, 0),
('a0000000-0000-0000-0000-000000000002', 'Individual', 'maria.santos@email.com',
 '$2a$11$abcdefgh', '23456789012', '{now.AddMonths(-2):yyyy-MM-dd HH:mm:ss}', NULL, 0),
('a0000000-0000-0000-0000-000000000003', 'Individual', 'pedro.oliveira@email.com',
 '$2a$11$abcdefgh', '34567890123', '{now.AddMonths(-1):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Corporate User (TPH: user_type = 'Corporate')
INSERT INTO users (id, user_type, email, password_hash, cnpj, company_name, created_at, updated_at, version)
VALUES ('a0000000-0000-0000-0000-000000000004', 'Corporate', 'contato@empresaabc.com.br',
 '$2a$11$abcdefgh', '12345678000190', 'Empresa ABC Ltda', '{now.AddMonths(-4):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Ativo 1 - Notebook Gaming
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status, 
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000001', 'Notebook Gaming Alienware m15 R7',
        'Notebook gamer de alto desempenho com RTX 3070, 32GB RAM, SSD 1TB. Estado de novo, apenas 3 meses de uso.',
        5000.00, 'BRL', 7000.00, 'BRL', 5400.00, 'BRL', 100.00, 'BRL',
        '{now.AddDays(-2):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(3):yyyy-MM-dd HH:mm:ss}', 'Active',
        'a0000000-0000-0000-0000-000000000001', NULL, '11111111-1111-1111-1111-111111111111',
        4, NULL, '{rulesJson}', '{now.AddDays(-3):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Ativo 2 - iPhone
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000002', 'iPhone 15 Pro Max 256GB - Titânio Natural',
        'iPhone 15 Pro Max na cor Titânio Natural, 256GB. Novo na caixa, lacrado.',
        6000.00, 'BRL', 7500.00, 'BRL', 6450.00, 'BRL', 150.00, 'BRL',
        '{now.AddDays(-1):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(5):yyyy-MM-dd HH:mm:ss}', 'Active',
        'a0000000-0000-0000-0000-000000000002', NULL, '11111111-1111-1111-1111-111111111111',
        3, NULL, '{rulesJson}', '{now.AddDays(-2):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Ativo 3 - Honda Civic
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000003', 'Honda Civic 2022 - Touring',
        'Honda Civic Touring 2022, top de linha. 15.000km rodados, único dono, revisões na concessionária.',
        115000.00, 'BRL', 130000.00, 'BRL', 121000.00, 'BRL', 1000.00, 'BRL',
        '{now.AddDays(-3):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(4):yyyy-MM-dd HH:mm:ss}', 'Active',
        'a0000000-0000-0000-0000-000000000003', NULL, '22222222-2222-2222-2222-222222222222',
        6, NULL, '{rulesJson}', '{now.AddDays(-4):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Agendado 1 - Arte
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000004', 'Pintura a Óleo - Paisagem Brasileira',
        'Obra original de artista renomado, pintura a óleo sobre tela 100x80cm. Assinada e com certificado de autenticidade.',
        8000.00, 'BRL', 12000.00, 'BRL', 8000.00, 'BRL', 500.00, 'BRL',
        '{now.AddDays(2):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(9):yyyy-MM-dd HH:mm:ss}', 'Scheduled',
        'a0000000-0000-0000-0000-000000000001', NULL, '33333333-3333-3333-3333-333333333333',
        0, NULL, '{rulesJson}', '{now.AddDays(1):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Agendado 2 - Quadrinhos
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000005', 'Coleção Completa de Quadrinhos Marvel Anos 80',
        'Coleção completa de quadrinhos Marvel da década de 80, todos em excelente estado de conservação.',
        3000.00, 'BRL', 5000.00, 'BRL', 3000.00, 'BRL', 200.00, 'BRL',
        '{now.AddDays(1):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(8):yyyy-MM-dd HH:mm:ss}', 'Scheduled',
        'a0000000-0000-0000-0000-000000000004', NULL, '44444444-4444-4444-4444-444444444444',
        0, NULL, '{rulesJson}', '{now:yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Finalizado 1 - PlayStation 5
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000006', 'PlayStation 5 + 3 Jogos',
        'Console PlayStation 5 edição padrão com 3 jogos AAA. Pouco uso, praticamente novo.',
        3000.00, 'BRL', 3500.00, 'BRL', 3800.00, 'BRL', 100.00, 'BRL',
        '{now.AddDays(-10):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(-3):yyyy-MM-dd HH:mm:ss}', 'Ended',
        'a0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111111',
        8, NULL, '{rulesJson}', '{now.AddDays(-11):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(-3):yyyy-MM-dd HH:mm:ss}', 0);

-- Leilão Ativo 4 - Relógio Rolex
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000007', 'Relógio Rolex Submariner - Edição Limitada',
        'Relógio Rolex Submariner, edição limitada, com certificado de autenticidade e caixa original.',
        50000.00, 'BRL', 65000.00, 'BRL', 54000.00, 'BRL', 2000.00, 'BRL',
        '{now.AddDays(-1):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(6):yyyy-MM-dd HH:mm:ss}', 'Active',
        'a0000000-0000-0000-0000-000000000004', NULL, '66666666-6666-6666-6666-666666666666',
        2, NULL, '{rulesJson}', '{now.AddDays(-2):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Ativo 5 - Yamaha MT-09
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000008', 'Yamaha MT-09 2023 - 0km',
        'Moto Yamaha MT-09 2023, zero quilômetro, cor preta, pronta entrega.',
        35000.00, 'BRL', 42000.00, 'BRL', 37000.00, 'BRL', 500.00, 'BRL',
        '{now.AddDays(-2):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(5):yyyy-MM-dd HH:mm:ss}', 'Active',
        'a0000000-0000-0000-0000-000000000003', NULL, '22222222-2222-2222-2222-222222222222',
        4, NULL, '{rulesJson}', '{now.AddDays(-3):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Agendado 3 - Apartamento
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000009', 'Apartamento 3 Quartos - Bairro Nobre São Paulo',
        'Apartamento 120m², 3 quartos, 2 vagas, vista privilegiada. Localização nobre em São Paulo.',
        800000.00, 'BRL', 950000.00, 'BRL', 800000.00, 'BRL', 10000.00, 'BRL',
        '{now.AddDays(5):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(15):yyyy-MM-dd HH:mm:ss}', 'Scheduled',
        'a0000000-0000-0000-0000-000000000002', NULL, '55555555-5555-5555-5555-555555555555',
        0, NULL, '{rulesJson}', '{now.AddDays(4):yyyy-MM-dd HH:mm:ss}', NULL, 0);

-- Leilão Draft 1 - MacBook Pro
INSERT INTO auctions (id, title, description, starting_price_amount, starting_price_currency,
                      reserve_price_amount, reserve_price_currency, current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, start_date, end_date, status,
                      seller_id, winner_id, category_id, total_bids, current_winning_bid_id,
                      rules, created_at, updated_at, version)
VALUES ('b0000000-0000-0000-0000-000000000010', 'MacBook Pro M3 Max 16"""" - 64GB RAM',
        'MacBook Pro com chip M3 Max, 16 polegadas, 64GB RAM, SSD 2TB. Perfeito para desenvolvimento e edição.',
        20000.00, 'BRL', 25000.00, 'BRL', 20000.00, 'BRL', 500.00, 'BRL',
        '{now.AddDays(10):yyyy-MM-dd HH:mm:ss}', '{now.AddDays(17):yyyy-MM-dd HH:mm:ss}', 'Draft',
        'a0000000-0000-0000-0000-000000000001', NULL, '11111111-1111-1111-1111-111111111111',
        0, NULL, '{rulesJson}', '{now.AddDays(9):yyyy-MM-dd HH:mm:ss}', NULL, 0);
";
    }
}
