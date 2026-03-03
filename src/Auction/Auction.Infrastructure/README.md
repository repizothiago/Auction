# 🏛️ Sistema de Leilão - Infrastructure Layer

## 📋 Visão Geral

Camada de Infrastructure implementada seguindo **Clean Architecture** e **DDD (Domain-Driven Design)** para o Sistema de Leilão.

### Tecnologias Utilizadas

- **PostgreSQL** - Banco de dados relacional (Write Database)
- **Redis** - Cache distribuído e leitura rápida
- **Kafka** - Message broker para Event-Driven Architecture
- **Entity Framework Core 9.0** - ORM para PostgreSQL
- **SignalR** - Notificações em tempo real
- **Docker Compose** - Orquestração da infraestrutura

---

## 🏗️ Estrutura do Projeto

```
Auction.Infrastructure/
├── Persistence/
│   ├── Configurations/         # EF Core Entity Configurations
│   │   ├── AuctionConfiguration.cs
│   │   ├── UserConfiguration.cs
│   │   ├── CategoryConfiguration.cs
│   │   └── ...
│   ├── Repositories/           # Implementações dos repositórios
│   │   ├── AuctionRepository.cs
│   │   ├── UserRepository.cs
│   │   └── CategoryRepository.cs
│   ├── Migrations/             # EF Core Migrations
│   ├── AppDbContext.cs         # DbContext principal
│   ├── AppDbContextFactory.cs  # Factory para design-time
│   ├── UnitOfWork.cs           # Unit of Work pattern
│   └── MIGRATIONS_GUIDE.md     # Guia de migrations
├── Caching/
│   └── RedisCacheService.cs    # Serviço de cache com Redis
├── Messaging/
│   ├── KafkaProducer.cs        # Producer Kafka
│   └── KafkaConsumerBase.cs    # Base class para consumers
├── SignalR/                     # Hubs SignalR (futuro)
└── DependencyInjection.cs      # Registro de serviços
```

---

## 🗄️ Banco de Dados - Estrutura

### Tabelas Criadas

#### **users** (Table Per Hierarchy - TPH)
Armazena usuários (Pessoa Física e Jurídica) em uma única tabela.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| id | UUID | PK |
| user_type | VARCHAR(50) | Discriminator: 'Individual' ou 'Corporate' |
| email | VARCHAR(255) | Email único |
| password_hash | VARCHAR(500) | Senha hasheada |
| cpf | VARCHAR(11) | CPF (apenas Individual) |
| cnpj | VARCHAR(14) | CNPJ (apenas Corporate) |
| company_name | VARCHAR(255) | Razão social (apenas Corporate) |
| created_at | TIMESTAMP | Data de criação |
| updated_at | TIMESTAMP | Última atualização |
| version | BIGINT | Optimistic Concurrency Control |

**Índices:**
- `idx_users_email` (UNIQUE)

---

#### **categories**
Categorias de leilão.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| id | UUID | PK |
| name | VARCHAR(100) | Nome único |
| description | TEXT | Descrição |
| created_at | TIMESTAMP | Data de criação |
| updated_at | TIMESTAMP | Última atualização |
| version | BIGINT | OCC |

**Índices:**
- `idx_categories_name` (UNIQUE)

**Seed Data:** 7 categorias pré-definidas

---

#### **auctions**
Leilões.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| id | UUID | PK |
| title | VARCHAR(255) | Título |
| description | TEXT | Descrição |
| starting_price_amount | DECIMAL(18,2) | Preço inicial |
| starting_price_currency | VARCHAR(3) | Moeda (BRL) |
| reserve_price_amount | DECIMAL(18,2) | Preço reserva |
| current_price_amount | DECIMAL(18,2) | Preço atual |
| bid_increment_amount | DECIMAL(18,2) | Incremento mínimo |
| buy_now_price_amount | DECIMAL(18,2) | Preço compra imediata (opcional) |
| start_date | TIMESTAMP | Data início |
| end_date | TIMESTAMP | Data fim |
| status | VARCHAR(50) | Status do leilão |
| seller_id | UUID | FK para users |
| winner_id | UUID | FK para users (nullable) |
| category_id | UUID | FK para categories |
| current_winning_bid_id | UUID | Último lance vencedor |
| total_bids | INT | Contador de lances |
| rules | JSONB | Regras do leilão (JSON) |
| created_at | TIMESTAMP | Data de criação |
| updated_at | TIMESTAMP | Última atualização |
| version | BIGINT | OCC |

**Índices:**
- `idx_auctions_status`
- `idx_auctions_end_date`
- `idx_auctions_seller`
- `idx_auctions_category`
- `idx_auctions_active_ending` (filtered: status = 'Active')

---

## 🔄 Padrões Implementados

### 1. **Repository Pattern**
Abstração de acesso a dados:
```csharp
public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Auction>> GetActiveAuctionsAsync(...);
    Task AddAsync(Auction auction, CancellationToken ct);
    void Update(Auction auction);
}
```

### 2. **Unit of Work Pattern**
Transações ACID:
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

### 3. **Optimistic Concurrency Control**
Controle de concorrência via `version`:
```csharp
builder.Property(a => a.Version)
    .IsConcurrencyToken();
```

### 4. **Value Objects como Owned Entities**
Money, Email, Password mapeados como owned:
```csharp
builder.OwnsOne(a => a.StartingPrice, money =>
{
    money.Property(m => m.Value).HasColumnName("starting_price_amount");
    money.Property(m => m.Currency).HasColumnName("starting_price_currency");
});
```

---

## 🚀 Quick Start

### 1. Pré-requisitos
- .NET 10 SDK
- Docker Desktop
- EF Core Tools: `dotnet tool install --global dotnet-ef`

### 2. Download das Imagens Docker (Recomendado)

Baixe todas as imagens necessárias (~2.4GB) antes de iniciar:

**Windows (PowerShell):**
```powershell
./download-docker-images.ps1
```

**Linux/Mac (Bash):**
```bash
chmod +x download-docker-images.sh
./download-docker-images.sh
```

### 3. Iniciar Infraestrutura

```powershell
# Opção 1: Script automatizado (RECOMENDADO)
./start-infrastructure.ps1

# Opção 2: Manual
docker-compose up -d
dotnet ef database update --project Auction.Infrastructure
```

### 4. Verificar

```powershell
# Status dos containers
docker-compose ps

# Testar PostgreSQL
docker exec -it auction-postgres psql -U postgres -d auction_db -c "\dt"

# Testar Redis
docker exec -it auction-redis redis-cli -a redis123 PING

# Testar Kafka
docker exec -it auction-kafka kafka-topics --bootstrap-server localhost:9092 --list
```

---

## 📦 Services & Ports

| Serviço | Porta | Acesso | Credenciais |
|---------|-------|--------|-------------|
| PostgreSQL | 5432 | `localhost:5432` | user: `postgres`<br>pass: `postgres` |
| Redis | 6379 | `localhost:6379` | pass: `redis123` |
| Kafka | 29092 | `localhost:29092` | - |
| pgAdmin | 5050 | http://localhost:5050 | email: `admin@auction.com`<br>pass: `admin` |
| Kafka UI | 8080 | http://localhost:8080 | - |
| Redis Commander | 8081 | http://localhost:8081 | - |

---

## 🔧 Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=auction_db;Username=postgres;Password=postgres",
    "Redis": "localhost:6379,password=redis123"
  },
  "Kafka": {
    "BootstrapServers": "localhost:29092"
  }
}
```

---

## 📚 Documentação Adicional

- **[DOCKER_SETUP.md](../DOCKER_SETUP.md)** - Guia completo de Docker e comandos úteis
- **[MIGRATIONS_GUIDE.md](Auction.Infrastructure/Persistence/MIGRATIONS_GUIDE.md)** - Guia de Entity Framework migrations
- **[AUCTION_SYSTEM_ARCHITECTURE.md](AUCTION_SYSTEM_ARCHITECTURE.md)** - Arquitetura completa do sistema

---

## 🧪 Próximos Passos

1. **Criar entidade Bid** - Tabela de lances com high-volume support
2. **Implementar Kafka Consumers** - Processar eventos de lances
3. **Implementar SignalR Hubs** - Notificações em tempo real
4. **Scripts de Seed** - Popular banco com dados de teste
5. **Health Checks** - Monitoramento de saúde dos serviços

---

## 🤝 Contribuindo

Este projeto segue:
- Clean Architecture
- Domain-Driven Design (DDD)
- SOLID Principles
- Repository Pattern
- Unit of Work Pattern

---

## 📄 Licença

[Adicionar licença aqui]
