# 🏛️ Sistema de Leilão - Auction System

Sistema de leilão online de alta performance projetado para suportar **3.000 usuários simultâneos por leilão** utilizando **Domain-Driven Design (DDD)**, **Clean Architecture** e **Event-Driven Architecture**.

---

## 🚀 Quick Start

### Setup Rápido (5 minutos)

**Para setup passo-a-passo detalhado, siga o [📋 Setup Checklist](SETUP_CHECKLIST.md)**

### 1️⃣ Pré-requisitos
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Git** - [Download](https://git-scm.com/)

### 2️⃣ Clone do Repositório
```bash
git clone https://github.com/repizothiago/Auction.git
cd Auction/src/Auction
```

### 3️⃣ Instalação das Ferramentas
```powershell
# EF Core Tools (para migrations)
dotnet tool install --global dotnet-ef

# Verificar instalação
dotnet ef --version
```

### 4️⃣ Download das Imagens Docker (⭐ Recomendado)
```powershell
# Windows
./download-docker-images.ps1

# Linux/Mac
chmod +x download-docker-images.sh
./download-docker-images.sh
```

### 5️⃣ Iniciar Infraestrutura
```powershell
./start-infrastructure.ps1
```

### 6️⃣ Executar a API
```powershell
dotnet run --project Auction.Api
```

### 7️⃣ Acessar Serviços

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **API** | https://localhost:7001 | - |
| **Swagger** | https://localhost:7001/swagger | - |
| **pgAdmin** | http://localhost:5050 | admin@auction.com / admin |
| **Kafka UI** | http://localhost:8080 | - |
| **Redis Commander** | http://localhost:8081 | - |

✅ **Pronto! Sistema funcionando!**

---

## 📋 Estrutura do Projeto

```
Auction/
├── src/Auction/
│   ├── Auction.Api/                  # Apresentação (REST API)
│   ├── Auction.Application/          # Application Layer (Commands, Queries)
│   ├── Auction.Domain/               # Domain Layer (Aggregates, Entities, VOs)
│   ├── Auction.Infrastructure/       # Infrastructure (DB, Cache, Messaging)
│   ├── Auction.SharedKernel/         # Shared Kernel (Result, DomainEvents)
│   │
│   ├── docker-compose.yml            # Infraestrutura Docker
│   ├── download-docker-images.ps1    # Script download imagens
│   ├── start-infrastructure.ps1      # Script inicialização
│   │
│   ├── DOCKER_SETUP.md               # 📚 Guia completo Docker
│   ├── SCRIPTS_GUIDE.md              # 📚 Guia de scripts
│   └── AUCTION_SYSTEM_ARCHITECTURE.md # 📚 Arquitetura completa
│
└── README.md                          # Este arquivo
```

---

## 🏗️ Arquitetura

### Clean Architecture + DDD

```
┌─────────────────────────────────────────────┐
│           PRESENTATION LAYER                 │
│     (API Controllers, SignalR Hubs)         │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│           APPLICATION LAYER                  │
│   (Commands, Queries, Handlers, DTOs)      │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│             DOMAIN LAYER                     │
│  (Aggregates, Entities, Value Objects)     │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│          INFRASTRUCTURE LAYER                │
│   (PostgreSQL, Redis, Kafka, EF Core)      │
└─────────────────────────────────────────────┘
```

### Tecnologias

**Backend:**
- .NET 10 (C#)
- Entity Framework Core 9.0
- PostgreSQL 16
- Redis 7
- Kafka 7.5

**Padrões:**
- Domain-Driven Design (DDD)
- Clean Architecture
- CQRS (Command Query Responsibility Segregation)
- Event-Driven Architecture
- Repository Pattern
- Unit of Work Pattern
- Optimistic Concurrency Control

---

## 📚 Documentação

### 📖 Guias Principais

| Documento | Descrição |
|-----------|-----------|
| **[DOCKER_SETUP.md](DOCKER_SETUP.md)** | Guia completo de Docker, comandos e troubleshooting |
| **[SCRIPTS_GUIDE.md](SCRIPTS_GUIDE.md)** | Documentação de todos os scripts automatizados |
| **[SETUP_CHECKLIST.md](SETUP_CHECKLIST.md)** | ✅ Checklist passo-a-passo de configuração inicial |
| **[AUCTION_SYSTEM_ARCHITECTURE.md](AUCTION_SYSTEM_ARCHITECTURE.md)** | Arquitetura detalhada do sistema completo |
| **[Infrastructure/README.md](Auction.Infrastructure/README.md)** | Documentação da camada Infrastructure |
| **[Infrastructure/MIGRATIONS_GUIDE.md](Auction.Infrastructure/Persistence/MIGRATIONS_GUIDE.md)** | Guia de Entity Framework migrations |
| **[Infrastructure/IMPLEMENTATION_SUMMARY.md](Auction.Infrastructure/IMPLEMENTATION_SUMMARY.md)** | Resumo da implementação |

### 🎓 Conceitos

- **Domain Model** - Ver `Auction.Domain/`
- **Aggregates** - Auction, User, Bid
- **Value Objects** - Money, Email, Password, CPF, CNPJ
- **Repository Pattern** - Ver `Auction.Infrastructure/Persistence/Repositories/`
- **Event-Driven** - Ver `Auction.Infrastructure/Messaging/`

---

## 🐳 Docker Services

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| **PostgreSQL** | 5432 | Banco de dados principal (Write) |
| **Redis** | 6379 | Cache distribuído e Read DB |
| **Kafka** | 29092 | Message broker para eventos |
| **Zookeeper** | 2181 | Coordenação do Kafka |
| **pgAdmin** | 5050 | Interface web PostgreSQL |
| **Kafka UI** | 8080 | Interface web Kafka |
| **Redis Commander** | 8081 | Interface web Redis |

---

## 🛠️ Scripts Automatizados

### 📥 Download de Imagens
```powershell
# Baixa todas as imagens Docker necessárias (~2.4GB)
./download-docker-images.ps1
```

### 🚀 Iniciar Infraestrutura
```powershell
# Inicia containers + aplica migrations
./start-infrastructure.ps1
```

### 🔧 Comandos Manuais
```powershell
# Iniciar containers
docker-compose up -d

# Ver logs
docker-compose logs -f

# Parar containers
docker-compose stop

# Remover tudo (⚠️ apaga dados)
docker-compose down -v

# Aplicar migrations
dotnet ef database update --project Auction.Infrastructure

# Criar nova migration
dotnet ef migrations add MigrationName --project Auction.Infrastructure --output-dir Persistence/Migrations
```

---

## 🧪 Testes

```powershell
# Executar todos os testes
dotnet test

# Com coverage
dotnet test --collect:"XPlat Code Coverage"

# Testes específicos
dotnet test --filter "FullyQualifiedName~Auction.Domain.Tests"
```

---

## 🗄️ Database Schema

### Tabelas Criadas

#### **users** (Table Per Hierarchy)
- Usuários Pessoa Física e Jurídica
- CPF/CNPJ como Value Objects
- Email único com índice

#### **categories**
- Categorias de leilão
- 7 categorias pré-definidas (seed)

#### **auctions**
- Leilões completos
- Value Objects: Money, AuctionRules
- Optimistic Concurrency Control
- Índices de performance

### Migrations

```powershell
# Listar migrations
dotnet ef migrations list --project Auction.Infrastructure

# Ver SQL que será executado
dotnet ef migrations script --project Auction.Infrastructure

# Aplicar ao banco
dotnet ef database update --project Auction.Infrastructure

# Reverter migration
dotnet ef database update PreviousMigrationName --project Auction.Infrastructure
```

---

## 🎯 Roadmap

### ✅ Concluído (v1.0)
- [x] Domain Layer (Aggregates, VOs, Events)
- [x] Infrastructure Layer (PostgreSQL, Redis, Kafka)
- [x] Entity Framework Core Configurations
- [x] Migrations iniciais
- [x] Docker Compose setup
- [x] Scripts automatizados
- [x] Documentação completa

### 🚧 Em Desenvolvimento (v1.1)
- [ ] Application Layer (Commands, Queries, Handlers)
- [ ] Bid Entity e processamento
- [ ] Kafka Consumers
- [ ] SignalR Hubs para real-time
- [ ] API Controllers

### 📋 Planejado (v2.0)
- [ ] Authentication & Authorization (JWT)
- [ ] CQRS completo
- [ ] Event Sourcing para Bids
- [ ] Integration Tests
- [ ] Load Testing (3k users)
- [ ] CI/CD Pipeline

---

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch: `git checkout -b feature/MinhaFeature`
3. Commit suas mudanças: `git commit -m 'Add: MinhaFeature'`
4. Push para a branch: `git push origin feature/MinhaFeature`
5. Abra um Pull Request

### Padrões de Commit
```
Add: Nova funcionalidade
Fix: Correção de bug
Update: Atualização de código existente
Refactor: Refatoração sem mudança de comportamento
Docs: Apenas documentação
Test: Adição ou correção de testes
```

---

## 🐛 Troubleshooting

### Docker não inicia
```powershell
# Verificar se Docker Desktop está rodando
docker ps

# Verificar logs
docker-compose logs
```

### Migrations falham
```powershell
# Verificar connection string no appsettings.json
# Verificar se PostgreSQL está rodando
docker-compose ps postgres

# Aplicar com verbose
dotnet ef database update --project Auction.Infrastructure --verbose
```

### Porta já em uso
```powershell
# Verificar processos
netstat -ano | findstr "5432"
netstat -ano | findstr "6379"
netstat -ano | findstr "29092"

# Matar processo (substitua PID)
taskkill /F /PID <PID>
```

### Performance do Docker
```powershell
# Ver uso de recursos
docker stats

# Limpar recursos não usados
docker system prune -a
```

---

## 📞 Suporte

- **Issues**: [GitHub Issues](https://github.com/repizothiago/Auction/issues)
- **Documentação**: Ver seção [📚 Documentação](#-documentação)
- **Email**: [Adicionar email de contato]

---

## 📄 Licença

[Adicionar licença - MIT, Apache 2.0, etc]

---

## 👥 Autores

- **Thiago Repizo** - [@repizothiago](https://github.com/repizothiago)

---

## ⭐ Star History

Se este projeto foi útil para você, considere dar uma ⭐!

---

**Última atualização:** 2025  
**Versão:** 1.0.0  
**Status:** 🚧 Em Desenvolvimento
