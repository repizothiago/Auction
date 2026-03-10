-- ===================================================
-- SEED DATA para Sistema de Leilões
-- ===================================================
-- Senha para todos os usuários: Password123
-- Hash: $2a$11$N9qo8uLOickgx2ZMRZoMye
-- ===================================================

-- ===================================================
-- 1. INSERIR USUÁRIOS (7 usuários)
-- ===================================================

-- Pessoas Físicas (4 usuários)
INSERT INTO users (id, user_type, email, password_hash, cpf, cnpj, company_name, created_at, updated_at, version)
VALUES 
('10000000-0000-0000-0000-000000000001', 'Individual', 'joao.silva@email.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye', '12345678901', NULL, NULL, NOW() - INTERVAL '3 months', NULL, 0),
('10000000-0000-0000-0000-000000000002', 'Individual', 'maria.santos@email.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye', '23456789012', NULL, NULL, NOW() - INTERVAL '3 months', NULL, 0),
('10000000-0000-0000-0000-000000000003', 'Individual', 'pedro.oliveira@email.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye', '34567890123', NULL, NULL, NOW() - INTERVAL '3 months', NULL, 0),
('10000000-0000-0000-0000-000000000004', 'Individual', 'ana.costa@email.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye', '45678901234', NULL, NULL, NOW() - INTERVAL '3 months', NULL, 0);

-- Pessoas Jurídicas (3 empresas)
INSERT INTO users (id, user_type, email, password_hash, cpf, cnpj, company_name, created_at, updated_at, version)
VALUES 
('20000000-0000-0000-0000-000000000001', 'Corporate', 'contato@techleiloes.com.br', '$2a$11$N9qo8uLOickgx2ZMRZoMye', NULL, '12345678000190', 'Tech Leilões LTDA', NOW() - INTERVAL '2 months', NULL, 0),
('20000000-0000-0000-0000-000000000002', 'Corporate', 'vendas@artegaleria.com.br', '$2a$11$N9qo8uLOickgx2ZMRZoMye', NULL, '98765432000101', 'Arte Galeria S.A.', NOW() - INTERVAL '2 months', NULL, 0),
('20000000-0000-0000-0000-000000000003', 'Corporate', 'contato@veiculospremium.com.br', '$2a$11$N9qo8uLOickgx2ZMRZoMye', NULL, '11122233000144', 'Veículos Premium LTDA', NOW() - INTERVAL '2 months', NULL, 0);

-- ===================================================
-- 2. INSERIR LEILÕES (8 leilões)
-- ===================================================

INSERT INTO auctions (
    id, title, description, 
    starting_price_amount, starting_price_currency, 
    reserve_price_amount, reserve_price_currency, 
    buy_now_price_amount, buy_now_price_currency,
    current_price_amount, current_price_currency, 
    bid_increment_amount, bid_increment_currency,
    start_date, end_date, status, 
    seller_id, winner_id, category_id, 
    total_bids, current_winning_bid_id, 
    rules, created_at, updated_at, version
)
VALUES 
-- Leilão 1: Laptop Dell (ATIVO)
('30000000-0000-0000-0000-000000000001', 
 'Laptop Dell XPS 15 - i7 16GB RAM', 
 'Notebook Dell XPS 15 polegadas, processador Intel i7 11ª geração, 16GB RAM, SSD 512GB, placa de vídeo dedicada GTX 1650. Estado de conservação excelente.',
 2500.00, 'BRL', 2500.00, 'BRL', 3000.00, 'BRL', 2500.00, 'BRL', 50.00, 'BRL',
 NOW() - INTERVAL '2 days', NOW() + INTERVAL '5 days', 'Active', 
 '20000000-0000-0000-0000-000000000001', NULL, '1563318e-7548-4620-a936-9d0ed752f2bc', 
 15, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '1 month', NULL, 0),

-- Leilão 2: iPhone 14 Pro (ATIVO)
('30000000-0000-0000-0000-000000000002', 
 'iPhone 14 Pro Max 256GB - Lacrado',
 'iPhone 14 Pro Max 256GB, cor Space Black, lacrado na caixa. Garantia Apple de 1 ano. Acompanha todos os acessórios originais.',
 4500.00, 'BRL', 4500.00, 'BRL', 5500.00, 'BRL', 4800.00, 'BRL', 100.00, 'BRL',
 NOW() - INTERVAL '1 day', NOW() + INTERVAL '3 days', 'Active',
 '20000000-0000-0000-0000-000000000001', NULL, '1563318e-7548-4620-a936-9d0ed752f2bc', 
 8, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '1 month', NULL, 0),

-- Leilão 3: Honda Civic (ATIVO)
('30000000-0000-0000-0000-000000000003', 
 'Honda Civic Touring 2020 - Apenas 30.000 km',
 'Honda Civic Touring 2.0 16V Flex Aut. 2020, cor prata, 30.000 km rodados, único dono, manual e chave reserva, revisões em concessionária. IPVA 2024 pago.',
 85000.00, 'BRL', 85000.00, 'BRL', 95000.00, 'BRL', 87500.00, 'BRL', 500.00, 'BRL',
 NOW() - INTERVAL '3 days', NOW() + INTERVAL '7 days', 'Active',
 '20000000-0000-0000-0000-000000000003', NULL, '28477285-2e12-43a6-8270-de0bf9519d20', 
 22, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '2 months', NULL, 0),

-- Leilão 4: Pintura (AGENDADO)
('30000000-0000-0000-0000-000000000004', 
 'Pintura a Óleo ''Pôr do Sol'' - Artista Renomado',
 'Obra de arte original, pintura a óleo sobre tela 100x80cm, assinada pelo artista brasileiro João Mendes. Acompanha certificado de autenticidade.',
 15000.00, 'BRL', 15000.00, 'BRL', 20000.00, 'BRL', 15000.00, 'BRL', 500.00, 'BRL',
 NOW() + INTERVAL '2 days', NOW() + INTERVAL '10 days', 'Scheduled',
 '20000000-0000-0000-0000-000000000002', NULL, '984bc72b-4c10-4826-8024-6c478f0c3c60', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '1 month', NULL, 0),

-- Leilão 5: Colar de Diamantes (AGENDADO)
('30000000-0000-0000-0000-000000000005', 
 'Colar de Diamantes 18K - 2 Quilates',
 'Colar em ouro 18K com diamantes naturais totalizando 2 quilates. Design exclusivo, acompanha certificado gemológico e estojo de luxo.',
 35000.00, 'BRL', 35000.00, 'BRL', 45000.00, 'BRL', 35000.00, 'BRL', 1000.00, 'BRL',
 NOW() + INTERVAL '1 day', NOW() + INTERVAL '8 days', 'Scheduled',
 '20000000-0000-0000-0000-000000000002', NULL, '71b730fb-7549-4cb2-bdb2-091b0e7652bd', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '14 days', NULL, 0),

-- Leilão 6: Apartamento (AGENDADO)
('30000000-0000-0000-0000-000000000006', 
 'Apartamento 3 Quartos - Jardins, São Paulo/SP',
 'Apartamento 120m², 3 quartos sendo 1 suíte, 2 vagas de garagem, sacada gourmet. Edifício com piscina, academia e salão de festas. Localização privilegiada nos Jardins.',
 850000.00, 'BRL', 850000.00, 'BRL', 950000.00, 'BRL', 850000.00, 'BRL', 5000.00, 'BRL',
 NOW() + INTERVAL '5 days', NOW() + INTERVAL '15 days', 'Scheduled',
 '10000000-0000-0000-0000-000000000001', NULL, '7f7fa068-16c8-43e0-b2c5-20e50311d474', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '7 days', NULL, 0),

-- Leilão 7: Smart TV (FINALIZADO)
('30000000-0000-0000-0000-000000000007', 
 'Smart TV Samsung 65'''' QLED 4K',
 'Smart TV Samsung 65 polegadas, tecnologia QLED, resolução 4K, HDR, 120Hz. Nota fiscal e garantia de 1 ano. Perfeito estado.',
 3500.00, 'BRL', 3500.00, 'BRL', 4500.00, 'BRL', 4200.00, 'BRL', 100.00, 'BRL',
 NOW() - INTERVAL '10 days', NOW() - INTERVAL '3 days', 'Ended',
 '20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002', '1563318e-7548-4620-a936-9d0ed752f2bc', 
 28, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '3 months', NULL, 0),

-- Leilão 8: PlayStation 5 (FINALIZADO)
('30000000-0000-0000-0000-000000000008', 
 'PlayStation 5 + 2 Controles + 5 Jogos',
 'Console PlayStation 5 versão com leitor de disco, 2 controles DualSense, 5 jogos físicos (God of War, Spider-Man, Horizon, FIFA 24, GT7). Garantia até 2025.',
 2800.00, 'BRL', 2800.00, 'BRL', 3500.00, 'BRL', 3400.00, 'BRL', 50.00, 'BRL',
 NOW() - INTERVAL '15 days', NOW() - INTERVAL '5 days', 'Ended',
 '20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000003', '1563318e-7548-4620-a936-9d0ed752f2bc', 
 35, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '3 months', NULL, 0);

-- ===================================================
-- RESUMO DOS DADOS INSERIDOS
-- ===================================================
-- ✅ 7 Usuários criados
--    - 4 Pessoas Físicas (Individual)
--    - 3 Empresas (Corporate)
--    - Senha para todos: Password123
--
-- ✅ 8 Leilões criados
--    - 3 Leilões ATIVOS (recebendo lances)
--    - 3 Leilões AGENDADOS (futuros)
--    - 2 Leilões FINALIZADOS (com vencedores)
-- ===================================================
