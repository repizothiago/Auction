CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE TABLE categories (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        description text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        version bigint NOT NULL,
        CONSTRAINT "PK_categories" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        email character varying(255) NOT NULL,
        password_hash character varying(500) NOT NULL,
        user_type character varying(13) NOT NULL,
        cnpj character varying(14),
        company_name character varying(255),
        cpf character varying(11),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        version bigint NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE TABLE auctions (
        id uuid NOT NULL,
        title character varying(255) NOT NULL,
        description text NOT NULL,
        starting_price_amount numeric(18,2) NOT NULL,
        starting_price_currency character varying(3) NOT NULL DEFAULT 'BRL',
        reserve_price_amount numeric(18,2) NOT NULL,
        reserve_price_currency character varying(3) NOT NULL DEFAULT 'BRL',
        buy_now_price_amount numeric(18,2),
        buy_now_price_currency character varying(3),
        current_price_amount numeric(18,2) NOT NULL,
        current_price_currency character varying(3) NOT NULL DEFAULT 'BRL',
        bid_increment_amount numeric(18,2) NOT NULL,
        bid_increment_currency character varying(3) NOT NULL DEFAULT 'BRL',
        start_date timestamp with time zone NOT NULL,
        end_date timestamp with time zone NOT NULL,
        status character varying(50) NOT NULL,
        seller_id uuid NOT NULL,
        winner_id uuid,
        category_id uuid NOT NULL,
        total_bids integer NOT NULL DEFAULT 0,
        current_winning_bid_id uuid,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        version bigint NOT NULL,
        rules jsonb NOT NULL,
        CONSTRAINT "PK_auctions" PRIMARY KEY (id),
        CONSTRAINT "FK_auctions_categories_category_id" FOREIGN KEY (category_id) REFERENCES categories (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    INSERT INTO categories (id, created_at, description, name, updated_at, version)
    VALUES ('1563318e-7548-4620-a936-9d0ed752f2bc', TIMESTAMPTZ '2026-03-04T01:16:58.871917Z', 'Dispositivos eletrônicos e gadgets', 'Eletrônicos', NULL, 0);
    INSERT INTO categories (id, created_at, description, name, updated_at, version)
    VALUES ('28477285-2e12-43a6-8270-de0bf9519d20', TIMESTAMPTZ '2026-03-04T01:16:58.872343Z', 'Carros, motos e outros veículos', 'Veículos', NULL, 0);
    INSERT INTO categories (id, created_at, description, name, updated_at, version)
    VALUES ('2a0e3fab-1ef8-4b95-8e4a-c10277e159e7', TIMESTAMPTZ '2026-03-04T01:16:58.872345Z', 'Itens antigos e vintage', 'Antiguidades', NULL, 0);
    INSERT INTO categories (id, created_at, description, name, updated_at, version)
    VALUES ('71b730fb-7549-4cb2-bdb2-091b0e7652bd', TIMESTAMPTZ '2026-03-04T01:16:58.872345Z', 'Joias e pedras preciosas', 'Joias', NULL, 0);
    INSERT INTO categories (id, created_at, description, name, updated_at, version)
    VALUES ('75bbd8cf-c16c-4428-a9ca-e39b732b5a84', TIMESTAMPTZ '2026-03-04T01:16:58.872345Z', 'Outras categorias', 'Outros', NULL, 0);
    INSERT INTO categories (id, created_at, description, name, updated_at, version)
    VALUES ('7f7fa068-16c8-43e0-b2c5-20e50311d474', TIMESTAMPTZ '2026-03-04T01:16:58.872344Z', 'Casas, apartamentos e terrenos', 'Imóveis', NULL, 0);
    INSERT INTO categories (id, created_at, description, name, updated_at, version)
    VALUES ('984bc72b-4c10-4826-8024-6c478f0c3c60', TIMESTAMPTZ '2026-03-04T01:16:58.872344Z', 'Obras de arte e colecionáveis', 'Arte', NULL, 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE INDEX idx_auctions_active_ending ON auctions (end_date, status) WHERE status = 'Active';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE INDEX idx_auctions_category ON auctions (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE INDEX idx_auctions_end_date ON auctions (end_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE INDEX idx_auctions_seller ON auctions (seller_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE INDEX idx_auctions_status ON auctions (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_categories_name ON categories (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_users_email ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260304011659_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260304011659_InitialCreate', '10.0.3');
    END IF;
END $EF$;
COMMIT;

