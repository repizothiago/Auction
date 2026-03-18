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
-- 2. INSERIR LEILÕES (15 leilões)
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
-- ========== LEILÕES ATIVOS (10 leilões) ==========

-- Leilão 1: Laptop Dell (ATIVO)
('30000000-0000-0000-0000-000000000001', 
 'Laptop Dell XPS 15 - i7 16GB RAM', 
 'Notebook Dell XPS 15 polegadas, processador Intel i7 11ª geração, 16GB RAM, SSD 512GB, placa de vídeo dedicada GTX 1650. Estado de conservação excelente.',
 2500.00, 'BRL', 2500.00, 'BRL', 3000.00, 'BRL', 2500.00, 'BRL', 50.00, 'BRL',
 NOW() - INTERVAL '2 days', NOW() + INTERVAL '5 days', 'Active', 
 '20000000-0000-0000-0000-000000000001', NULL, '62fb8846-ac7d-481f-8b91-e19941180753', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '1 month', NULL, 0),

-- Leilão 2: iPhone 14 Pro (ATIVO)
('30000000-0000-0000-0000-000000000002', 
 'iPhone 14 Pro Max 256GB - Lacrado',
 'iPhone 14 Pro Max 256GB, cor Space Black, lacrado na caixa. Garantia Apple de 1 ano. Acompanha todos os acessórios originais.',
 4500.00, 'BRL', 4500.00, 'BRL', 5500.00, 'BRL', 4500.00, 'BRL', 100.00, 'BRL',
 NOW() - INTERVAL '1 day', NOW() + INTERVAL '3 days', 'Active',
 '20000000-0000-0000-0000-000000000001', NULL, '62fb8846-ac7d-481f-8b91-e19941180753', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '1 month', NULL, 0),

-- Leilão 3: Honda Civic (ATIVO)
('30000000-0000-0000-0000-000000000003', 
 'Honda Civic Touring 2020 - Apenas 30.000 km',
 'Honda Civic Touring 2.0 16V Flex Aut. 2020, cor prata, 30.000 km rodados, único dono, manual e chave reserva, revisões em concessionária. IPVA 2024 pago.',
 85000.00, 'BRL', 85000.00, 'BRL', 95000.00, 'BRL', 85000.00, 'BRL', 500.00, 'BRL',
 NOW() - INTERVAL '3 days', NOW() + INTERVAL '7 days', 'Active',
 '20000000-0000-0000-0000-000000000003', NULL, '448fb831-8414-4181-81dd-318b5a569194', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '2 months', NULL, 0),

-- Leilão 4: MacBook Pro M2 (ATIVO)
('30000000-0000-0000-0000-000000000004', 
 'MacBook Pro M2 16GB 512GB - Novo',
 'MacBook Pro 2023, chip M2, 16GB RAM unificada, SSD 512GB, tela Retina 13.3". Lacrado na caixa com garantia Apple Brasil.',
 7500.00, 'BRL', 7500.00, 'BRL', 9000.00, 'BRL', 7500.00, 'BRL', 100.00, 'BRL',
 NOW() - INTERVAL '12 hours', NOW() + INTERVAL '4 days', 'Active',
 '20000000-0000-0000-0000-000000000001', NULL, '62fb8846-ac7d-481f-8b91-e19941180753', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '15 days', NULL, 0),

-- Leilão 5: Toyota Corolla (ATIVO)
('30000000-0000-0000-0000-000000000005', 
 'Toyota Corolla XEI 2021 - 25.000 km',
 'Toyota Corolla XEI 2.0 Flex Automático 2021, cor prata, 25.000 km, único dono, manual e chave reserva. Revisões em concessionária. IPVA 2024 pago.',
 92000.00, 'BRL', 92000.00, 'BRL', 105000.00, 'BRL', 92000.00, 'BRL', 1000.00, 'BRL',
 NOW() - INTERVAL '6 hours', NOW() + INTERVAL '6 days', 'Active',
 '20000000-0000-0000-0000-000000000003', NULL, '448fb831-8414-4181-81dd-318b5a569194', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '20 days', NULL, 0),

-- Leilão 6: Samsung Galaxy S23 Ultra (ATIVO)
('30000000-0000-0000-0000-000000000006', 
 'Samsung Galaxy S23 Ultra 512GB - Preto',
 'Samsung Galaxy S23 Ultra 512GB, cor Phantom Black, estado de novo. Acompanha caixa original, carregador e fone AKG.',
 4200.00, 'BRL', 4200.00, 'BRL', 5200.00, 'BRL', 4200.00, 'BRL', 100.00, 'BRL',
 NOW() - INTERVAL '18 hours', NOW() + INTERVAL '2 days', 'Active',
 '20000000-0000-0000-0000-000000000001', NULL, '62fb8846-ac7d-481f-8b91-e19941180753', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '10 days', NULL, 0),

-- Leilão 7: iPad Pro 12.9 (ATIVO)
('30000000-0000-0000-0000-000000000007', 
 'iPad Pro 12.9" M2 256GB + Apple Pencil',
 'iPad Pro 12.9 polegadas com chip M2, 256GB, Wi-Fi + Cellular. Acompanha Apple Pencil 2ª geração e Magic Keyboard. Estado impecável.',
 6500.00, 'BRL', 6500.00, 'BRL', 7800.00, 'BRL', 6500.00, 'BRL', 100.00, 'BRL',
 NOW() - INTERVAL '1 day', NOW() + INTERVAL '5 days', 'Active',
 '20000000-0000-0000-0000-000000000001', NULL, '62fb8846-ac7d-481f-8b91-e19941180753', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '18 days', NULL, 0),

-- Leilão 8: Hyundai HB20S (ATIVO)
('30000000-0000-0000-0000-000000000008', 
 'Hyundai HB20S Comfort Plus 2022 - 18.000 km',
 'Hyundai HB20S 1.0 Turbo Automático 2022, cor branco, 18.000 km, único dono. Central multimídia, câmera de ré, sensores. IPVA 2024 pago.',
 68000.00, 'BRL', 68000.00, 'BRL', 75000.00, 'BRL', 68000.00, 'BRL', 500.00, 'BRL',
 NOW() - INTERVAL '4 hours', NOW() + INTERVAL '8 days', 'Active',
 '20000000-0000-0000-0000-000000000003', NULL, '448fb831-8414-4181-81dd-318b5a569194', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '5 days', NULL, 0),

-- Leilão 9: Relógio Rolex Submariner (ATIVO)
('30000000-0000-0000-0000-000000000009', 
 'Relógio Rolex Submariner Date - Aço Inox',
 'Relógio Rolex Submariner Date, caixa em aço inoxidável 904L, mostrador preto, movimento automático. Ano 2020, acompanha caixa, documentos e certificado de autenticidade.',
 55000.00, 'BRL', 55000.00, 'BRL', 68000.00, 'BRL', 55000.00, 'BRL', 1000.00, 'BRL',
 NOW() - INTERVAL '8 hours', NOW() + INTERVAL '10 days', 'Active',
 '20000000-0000-0000-0000-000000000002', NULL, 'ddebd330-f35b-4e8f-b820-e110168be0bf', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '12 days', NULL, 0),

-- Leilão 10: Câmera Canon EOS R6 (ATIVO)
('30000000-0000-0000-0000-000000000010', 
 'Câmera Canon EOS R6 + Lente RF 24-105mm',
 'Câmera mirrorless Canon EOS R6, sensor full-frame 20MP, vídeo 4K 60fps. Acompanha lente RF 24-105mm f/4-7.1 IS STM, carregador, bateria extra e bolsa.',
 12500.00, 'BRL', 12500.00, 'BRL', 15000.00, 'BRL', 12500.00, 'BRL', 200.00, 'BRL',
 NOW() - INTERVAL '2 hours', NOW() + INTERVAL '9 days', 'Active',
 '20000000-0000-0000-0000-000000000001', NULL, '62fb8846-ac7d-481f-8b91-e19941180753', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '8 days', NULL, 0),

-- ========== LEILÕES AGENDADOS (3 leilões) ==========

-- Leilão 11: Pintura (AGENDADO)
('30000000-0000-0000-0000-000000000011', 
 'Pintura a Óleo "Pôr do Sol" - Artista Renomado',
 'Obra de arte original, pintura a óleo sobre tela 100x80cm, assinada pelo artista brasileiro João Mendes. Acompanha certificado de autenticidade.',
 15000.00, 'BRL', 15000.00, 'BRL', 20000.00, 'BRL', 15000.00, 'BRL', 500.00, 'BRL',
 NOW() + INTERVAL '2 days', NOW() + INTERVAL '12 days', 'Scheduled',
 '20000000-0000-0000-0000-000000000002', NULL, '09ede924-5eac-4ccc-88ef-bedd335ece17', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '1 month', NULL, 0),

-- Leilão 12: Colar de Diamantes (AGENDADO)
('30000000-0000-0000-0000-000000000012', 
 'Colar de Diamantes 18K - 2 Quilates',
 'Colar em ouro 18K com diamantes naturais totalizando 2 quilates. Design exclusivo, acompanha certificado gemológico e estojo de luxo.',
 35000.00, 'BRL', 35000.00, 'BRL', 45000.00, 'BRL', 35000.00, 'BRL', 1000.00, 'BRL',
 NOW() + INTERVAL '1 day', NOW() + INTERVAL '11 days', 'Scheduled',
 '20000000-0000-0000-0000-000000000002', NULL, 'ddebd330-f35b-4e8f-b820-e110168be0bf', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '14 days', NULL, 0),

-- Leilão 13: Apartamento (AGENDADO)
('30000000-0000-0000-0000-000000000013', 
 'Apartamento 3 Quartos - Jardins, São Paulo/SP',
 'Apartamento 120m², 3 quartos sendo 1 suíte, 2 vagas de garagem, sacada gourmet. Edifício com piscina, academia e salão de festas. Localização privilegiada nos Jardins.',
 850000.00, 'BRL', 850000.00, 'BRL', 950000.00, 'BRL', 850000.00, 'BRL', 5000.00, 'BRL',
 NOW() + INTERVAL '5 days', NOW() + INTERVAL '20 days', 'Scheduled',
 '10000000-0000-0000-0000-000000000001', NULL, 'c9d47c38-d3ea-423d-a089-5ba55e14055e', 
 0, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '7 days', NULL, 0),

-- ========== LEILÕES FINALIZADOS (2 leilões) ==========

-- Leilão 14: Smart TV (FINALIZADO)
('30000000-0000-0000-0000-000000000014', 
 'Smart TV Samsung 65" QLED 4K',
 'Smart TV Samsung 65 polegadas, tecnologia QLED, resolução 4K, HDR, 120Hz. Nota fiscal e garantia de 1 ano. Perfeito estado.',
 3500.00, 'BRL', 3500.00, 'BRL', 4500.00, 'BRL', 4200.00, 'BRL', 100.00, 'BRL',
 NOW() - INTERVAL '10 days', NOW() - INTERVAL '3 days', 'Ended',
 '20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002', '62fb8846-ac7d-481f-8b91-e19941180753', 
 28, NULL,
 '{"ExtensionTime":"00:05:00","ExtensionWindow":"00:05:00","MaxBidsPerUser":10,"AllowProxyBids":true}'::jsonb,
 NOW() - INTERVAL '3 months', NULL, 0),

-- Leilão 15: PlayStation 5 (FINALIZADO)
('30000000-0000-0000-0000-000000000015', 
 'PlayStation 5 + 2 Controles + 5 Jogos',
 'Console PlayStation 5 versão com leitor de disco, 2 controles DualSense, 5 jogos físicos (God of War, Spider-Man, Horizon, FIFA 24, GT7). Garantia até 2025.',
 2800.00, 'BRL', 2800.00, 'BRL', 3500.00, 'BRL', 3400.00, 'BRL', 50.00, 'BRL',
 NOW() - INTERVAL '15 days', NOW() - INTERVAL '5 days', 'Ended',
 '20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000003', '62fb8846-ac7d-481f-8b91-e19941180753', 
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
-- ✅ 15 Leilões criados
--    - 10 Leilões ATIVOS (recebendo lances agora!)
--       • Eletrônicos: Laptop Dell, iPhone 14, MacBook Pro, Galaxy S23, iPad Pro, Câmera Canon
--       • Veículos: Honda Civic, Toyota Corolla, Hyundai HB20S
--       • Joias: Rolex Submariner
--    - 3 Leilões AGENDADOS (futuros)
--    - 2 Leilões FINALIZADOS (com vencedores)
-- ===================================================

