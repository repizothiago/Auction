# Entity Framework Core - Migrations Guide

## Pré-requisitos
- .NET 10 SDK instalado
- PostgreSQL instalado e rodando
- String de conexão configurada em appsettings.json

## Instalação do EF Core Tools

```powershell
dotnet tool install --global dotnet-ef
# Ou atualizar se já estiver instalado
dotnet tool update --global dotnet-ef
```

## Comandos de Migration

### 1. Criar Migration Inicial
```powershell
# A partir da pasta raiz do projeto (onde está a solution)
dotnet ef migrations add InitialCreate --project Auction.Infrastructure --startup-project Auction.Api --output-dir Persistence/Migrations
```

### 2. Visualizar SQL que será gerado
```powershell
dotnet ef migrations script --project Auction.Infrastructure --startup-project Auction.Api
```

### 3. Aplicar Migration ao Banco
```powershell
dotnet ef database update --project Auction.Infrastructure --startup-project Auction.Api
```

### 4. Remover última Migration (se necessário)
```powershell
dotnet ef migrations remove --project Auction.Infrastructure --startup-project Auction.Api
```

### 5. Listar Migrations
```powershell
dotnet ef migrations list --project Auction.Infrastructure --startup-project Auction.Api
```

## String de Conexão (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=auction_db;Username=postgres;Password=your_password",
    "Redis": "localhost:6379"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

## Estrutura do Banco de Dados

### Tabelas Criadas:
1. **users** - Table Per Hierarchy (TPH) para IndividualEntity e CorporateEntity
2. **categories** - Categorias de leilão
3. **auctions** - Leilões
4. **bids** - Lances (será criado em próxima fase)

### Índices Criados:
- Performance otimizada para queries de leilões ativos
- Índices únicos para email, CPF, CNPJ
- Índices compostos para filtros comuns

## Troubleshooting

### Erro: "No executable found matching command dotnet-ef"
Solução: Instale o EF Core tools globalmente

### Erro: "Your startup project doesn't reference Microsoft.EntityFrameworkCore.Design"
Solução: O pacote já está referenciado no Auction.Infrastructure.csproj

### Erro: "Unable to create an object of type 'AppDbContext'"
Solução: Certifique-se que o Auction.Api tem a string de conexão configurada
