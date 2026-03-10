-- ============================================
-- Script de Seed - Sistema de Leilões
-- ============================================

-- ============================================
-- LIMPEZA DE DADOS ANTIGOS
-- ============================================
-- IMPORTANTE: Isso apaga TODOS os dados das tabelas!
-- Comente essas linhas se quiser preservar dados existentes

TRUNCATE TABLE auctions CASCADE;
TRUNCATE TABLE users CASCADE;
TRUNCATE TABLE categories CASCADE;

-- ============================================
-- 1. CATEGORIES (6 categorias)
-- ============================================
INSERT INTO categories (id, name, description, created_at, updated_at, version) VALUES
('11111111-1111-1111-1111-111111111111', 'Eletrônicos', 'Dispositivos eletrônicos e tecnologia', '2024-01-01 00:00:00+00', NULL, 0),
('22222222-2222-2222-2222-222222222222', 'Veículos', 'Carros, motos e veículos em geral', '2024-01-01 00:00:00+00', NULL, 0),
('33333333-3333-3333-3333-333333333333', 'Arte', 'Obras de arte, pinturas e esculturas', '2024-01-01 00:00:00+00', NULL, 0),
('44444444-4444-4444-4444-444444444444', 'Colecionáveis', 'Itens colecionáveis e antiguidades', '2024-01-01 00:00:00+00', NULL, 0),
('55555555-5555-5555-5555-555555555555', 'Imóveis', 'Casas, apartamentos e terrenos', '2024-01-01 00:00:00+00', NULL, 0),
('66666666-6666-6666-6666-666666666666', 'Moda', 'Roupas, acessórios e joias', '2024-01-01 00:00:00+00', NULL, 0);

-- ============================================
-- 2. USERS (4 usuários - 3 Individual, 1 Corporate)
-- ============================================

-- Individual Users (TPH: user_type = 'Individual')
-- Nota: email e password_hash são colunas de Value Objects (OwnsOne)
INSERT INTO users (id, user_type, email, password_hash, cpf, created_at, updated_at, version) VALUES
('a0000000-0000-0000-0000-000000000001', 'Individual', 'joao.silva@email.com', '$2a$11$hashedpassword1', '12345678901', NOW() - INTERVAL '3 months', NULL, 0),
('a0000000-0000-0000-0000-000000000002', 'Individual', 'maria.santos@email.com', '$2a$11$hashedpassword2', '23456789012', NOW() - INTERVAL '2 months', NULL, 0),
('a0000000-0000-0000-0000-000000000003', 'Individual', 'pedro.oliveira@email.com', '$2a$11$hashedpassword3', '34567890123', NOW() - INTERVAL '1 month', NULL, 0);

-- Corporate User (TPH: user_type = 'Corporate')
-- Para Corporate, cnpj e company_name são obrigatórios, cpf deve ser NULL
INSERT INTO users (id, user_type, email, password_hash, cnpj, company_name, cpf, created_at, updated_at, version) VALUES
('a0000000-0000-0000-0000-000000000004', 'Corporate', 'contato@empresaabc.com.br', '$2a$11$hashedpassword4', '12345678000190', 'Empresa ABC Ltda', NULL, NOW() - INTERVAL '4 months', NULL, 0);

-- ============================================
-- 3. AUCTIONS (10 leilões com estados variados)
-- ============================================

-- JSON padrão para AuctionRules
-- {"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}

-- Leilão Ativo 1 - Notebook Gaming
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000001',
    'Notebook Gaming Alienware m15 R7',
    'Notebook gamer de alto desempenho com RTX 3070, 32GB RAM, SSD 1TB. Estado de novo, apenas 3 meses de uso.',
    5000.00, 'BRL', 7000.00, 'BRL', 5400.00, 'BRL', 100.00, 'BRL',
    NOW() - INTERVAL '2 days', NOW() + INTERVAL '3 days', 'Active',
    'a0000000-0000-0000-0000-000000000001', NULL, '11111111-1111-1111-1111-111111111111',
    4, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() - INTERVAL '3 days', NULL, 0
);

-- Leilão Ativo 2 - iPhone
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000002',
    'iPhone 15 Pro Max 256GB - Titânio Natural',
    'iPhone 15 Pro Max na cor Titânio Natural, 256GB. Novo na caixa, lacrado.',
    6000.00, 'BRL', 7500.00, 'BRL', 6450.00, 'BRL', 150.00, 'BRL',
    NOW() - INTERVAL '1 day', NOW() + INTERVAL '5 days', 'Active',
    'a0000000-0000-0000-0000-000000000002', NULL, '11111111-1111-1111-1111-111111111111',
    3, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() - INTERVAL '2 days', NULL, 0
);

-- Leilão Ativo 3 - Honda Civic
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000003',
    'Honda Civic 2022 - Touring',
    'Honda Civic Touring 2022, top de linha. 15.000km rodados, único dono, revisões na concessionária.',
    115000.00, 'BRL', 130000.00, 'BRL', 121000.00, 'BRL', 1000.00, 'BRL',
    NOW() - INTERVAL '3 days', NOW() + INTERVAL '4 days', 'Active',
    'a0000000-0000-0000-0000-000000000003', NULL, '22222222-2222-2222-2222-222222222222',
    6, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() - INTERVAL '4 days', NULL, 0
);

-- Leilão Ativo 4 - Relógio Rolex
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000004',
    'Relógio Rolex Submariner - Edição Limitada',
    'Relógio Rolex Submariner, edição limitada, com certificado de autenticidade e caixa original.',
    50000.00, 'BRL', 65000.00, 'BRL', 54000.00, 'BRL', 2000.00, 'BRL',
    NOW() - INTERVAL '1 day', NOW() + INTERVAL '6 days', 'Active',
    'a0000000-0000-0000-0000-000000000004', NULL, '66666666-6666-6666-6666-666666666666',
    2, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() - INTERVAL '2 days', NULL, 0
);

-- Leilão Ativo 5 - Yamaha MT-09
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000005',
    'Yamaha MT-09 2023 - 0km',
    'Moto Yamaha MT-09 2023, zero quilômetro, cor preta, pronta entrega.',
    35000.00, 'BRL', 42000.00, 'BRL', 37000.00, 'BRL', 500.00, 'BRL',
    NOW() - INTERVAL '2 days', NOW() + INTERVAL '5 days', 'Active',
    'a0000000-0000-0000-0000-000000000003', NULL, '22222222-2222-2222-2222-222222222222',
    4, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() - INTERVAL '3 days', NULL, 0
);

-- Leilão Agendado 1 - Arte
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000006',
    'Pintura a Óleo - Paisagem Brasileira',
    'Obra original de artista renomado, pintura a óleo sobre tela 100x80cm. Assinada e com certificado de autenticidade.',
    8000.00, 'BRL', 12000.00, 'BRL', 8000.00, 'BRL', 500.00, 'BRL',
    NOW() + INTERVAL '2 days', NOW() + INTERVAL '9 days', 'Scheduled',
    'a0000000-0000-0000-0000-000000000001', NULL, '33333333-3333-3333-3333-333333333333',
    0, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() + INTERVAL '1 day', NULL, 0
);

-- Leilão Agendado 2 - Quadrinhos
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000007',
    'Coleção Completa de Quadrinhos Marvel Anos 80',
    'Coleção completa de quadrinhos Marvel da década de 80, todos em excelente estado de conservação.',
    3000.00, 'BRL', 5000.00, 'BRL', 3000.00, 'BRL', 200.00, 'BRL',
    NOW() + INTERVAL '1 day', NOW() + INTERVAL '8 days', 'Scheduled',
    'a0000000-0000-0000-0000-000000000004', NULL, '44444444-4444-4444-4444-444444444444',
    0, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW(), NULL, 0
);

-- Leilão Agendado 3 - Apartamento
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000008',
    'Apartamento 3 Quartos - Bairro Nobre São Paulo',
    'Apartamento 120m², 3 quartos, 2 vagas, vista privilegiada. Localização nobre em São Paulo.',
    800000.00, 'BRL', 950000.00, 'BRL', 800000.00, 'BRL', 10000.00, 'BRL',
    NOW() + INTERVAL '5 days', NOW() + INTERVAL '15 days', 'Scheduled',
    'a0000000-0000-0000-0000-000000000002', NULL, '55555555-5555-5555-5555-555555555555',
    0, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() + INTERVAL '4 days', NULL, 0
);

-- Leilão Finalizado - PlayStation 5
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000009',
    'PlayStation 5 + 3 Jogos',
    'Console PlayStation 5 edição padrão com 3 jogos AAA. Pouco uso, praticamente novo.',
    3000.00, 'BRL', 3500.00, 'BRL', 3800.00, 'BRL', 100.00, 'BRL',
    NOW() - INTERVAL '10 days', NOW() - INTERVAL '3 days', 'Ended',
    'a0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111111',
    8, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() - INTERVAL '11 days', NOW() - INTERVAL '3 days', 0
);

-- Leilão Draft - MacBook Pro
INSERT INTO auctions (id, title, description, 
                      starting_price_amount, starting_price_currency, 
                      reserve_price_amount, reserve_price_currency, 
                      current_price_amount, current_price_currency,
                      bid_increment_amount, bid_increment_currency, 
                      start_date, end_date, status, 
                      seller_id, winner_id, category_id, 
                      total_bids, current_winning_bid_id, 
                      rules, created_at, updated_at, version)
VALUES (
    'b0000000-0000-0000-0000-000000000010',
    'MacBook Pro M3 Max 16" - 64GB RAM',
    'MacBook Pro com chip M3 Max, 16 polegadas, 64GB RAM, SSD 2TB. Perfeito para desenvolvimento e edição.',
    20000.00, 'BRL', 25000.00, 'BRL', 20000.00, 'BRL', 500.00, 'BRL',
    NOW() + INTERVAL '10 days', NOW() + INTERVAL '17 days', 'Draft',
    'a0000000-0000-0000-0000-000000000001', NULL, '11111111-1111-1111-1111-111111111111',
    0, NULL,
    '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":5,"AllowProxyBids":true}'::jsonb,
    NOW() + INTERVAL '9 days', NULL, 0
);

-- ============================================
-- Verificação dos dados inseridos
-- ============================================
SELECT 'Categories:', COUNT(*) FROM categories;
SELECT 'Users:', COUNT(*) FROM users;
SELECT 'Auctions:', COUNT(*) FROM auctions;
SELECT 'Auctions por status:', status, COUNT(*) FROM auctions GROUP BY status ORDER BY status;
