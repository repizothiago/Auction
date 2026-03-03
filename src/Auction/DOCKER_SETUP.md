# Scripts Docker Compose e Migrations - Sistema de Leilão

## 📥 Download de Imagens Docker (Recomendado Primeiro)

Antes de iniciar os serviços, é recomendado baixar todas as imagens necessárias:

### Windows (PowerShell)
```powershell
./download-docker-images.ps1
```

### Linux/Mac (Bash)
```bash
chmod +x download-docker-images.sh
./download-docker-images.sh
```

**Benefícios:**
- ✅ Download prévio de ~2.4GB de imagens
- ✅ Verifica atualizações disponíveis
- ✅ Mostra progresso detalhado
- ✅ Identifica falhas de download
- ✅ Economiza tempo no primeiro `docker-compose up`

---

## 🐳 Docker Compose - Iniciar Infraestrutura

### Iniciar todos os serviços
```powershell
docker-compose up -d
```

### Verificar status dos containers
```powershell
docker-compose ps
```

### Ver logs
```powershell
# Todos os serviços
docker-compose logs -f

# Serviço específico
docker-compose logs -f postgres
docker-compose logs -f redis
docker-compose logs -f kafka
```

### Parar serviços
```powershell
docker-compose stop
```

### Parar e remover containers
```powershell
docker-compose down
```

### Parar, remover containers E volumes (⚠️ apaga os dados)
```powershell
docker-compose down -v
```

---

## 📊 Serviços Disponíveis

| Serviço | Porta | URL / Conexão | Credenciais |
|---------|-------|---------------|-------------|
| **PostgreSQL** | 5432 | `localhost:5432` | user: `postgres`<br>password: `postgres`<br>database: `auction_db` |
| **Redis** | 6379 | `localhost:6379` | password: `redis123` |
| **Kafka** | 29092 | `localhost:29092` | - |
| **pgAdmin** | 5050 | http://localhost:5050 | email: `admin@auction.com`<br>password: `admin` |
| **Kafka UI** | 8080 | http://localhost:8080 | - |
| **Redis Commander** | 8081 | http://localhost:8081 | - |

---

## 🗄️ Migrations - Entity Framework Core

### Criar uma nova migration
```powershell
dotnet ef migrations add NomeDaMigration --project Auction.Infrastructure --output-dir Persistence/Migrations
```

### Aplicar migrations ao banco de dados
```powershell
dotnet ef database update --project Auction.Infrastructure
```

### Reverter para uma migration específica
```powershell
dotnet ef database update NomeDaMigration --project Auction.Infrastructure
```

### Remover última migration (se ainda não foi aplicada)
```powershell
dotnet ef migrations remove --project Auction.Infrastructure
```

### Gerar script SQL das migrations
```powershell
# Todas as migrations
dotnet ef migrations script --project Auction.Infrastructure --output migrations.sql

# De uma migration específica até outra
dotnet ef migrations script FromMigration ToMigration --project Auction.Infrastructure --output migrations.sql

# Da última migration aplicada até a mais recente
dotnet ef migrations script --project Auction.Infrastructure --idempotent --output migrations.sql
```

### Listar todas as migrations
```powershell
dotnet ef migrations list --project Auction.Infrastructure
```

### Ver informações do DbContext
```powershell
dotnet ef dbcontext info --project Auction.Infrastructure
```

---

## 🚀 Fluxo Completo de Inicialização

### 0. (Opcional mas Recomendado) Download prévio das imagens
```powershell
# Windows
./download-docker-images.ps1

# Linux/Mac
./download-docker-images.sh
```

### 1. Iniciar infraestrutura
```powershell
# Subir containers
docker-compose up -d

# Aguardar healthcheck (opcional - 30 segundos)
Start-Sleep -Seconds 30

# Verificar se tudo está rodando
docker-compose ps
```

### 2. Aplicar migrations
```powershell
# A partir da pasta raiz da solution
dotnet ef database update --project Auction.Infrastructure
```

### 3. Verificar banco de dados
```powershell
# Opção 1: pgAdmin
# Acesse: http://localhost:5050
# Adicione servidor:
#   Host: postgres (ou host.docker.internal no Windows/Mac)
#   Port: 5432
#   Database: auction_db
#   Username: postgres
#   Password: postgres

# Opção 2: Linha de comando
docker exec -it auction-postgres psql -U postgres -d auction_db -c "\dt"
```

### 4. Verificar Redis
```powershell
# Redis Commander
# Acesse: http://localhost:8081

# Ou via CLI
docker exec -it auction-redis redis-cli -a redis123 PING
```

### 5. Verificar Kafka
```powershell
# Kafka UI
# Acesse: http://localhost:8080

# Ou via CLI - listar tópicos
docker exec -it auction-kafka kafka-topics --bootstrap-server localhost:9092 --list
```

---

## 🧪 Comandos Úteis de Desenvolvimento

### Popular banco com dados de teste (seed)
```powershell
# TODO: Criar script de seed
# dotnet run --project Auction.Api -- --seed
```

### Resetar banco de dados completamente
```powershell
# Remover containers e volumes
docker-compose down -v

# Subir novamente
docker-compose up -d

# Aguardar healthcheck
Start-Sleep -Seconds 30

# Aplicar migrations
dotnet ef database update --project Auction.Infrastructure
```

### Backup do banco de dados
```powershell
# Backup
docker exec auction-postgres pg_dump -U postgres auction_db > backup_$(Get-Date -Format "yyyyMMdd_HHmmss").sql

# Restore
Get-Content backup.sql | docker exec -i auction-postgres psql -U postgres -d auction_db
```

### Limpar dados do Redis
```powershell
docker exec -it auction-redis redis-cli -a redis123 FLUSHALL
```

### Criar tópicos Kafka manualmente
```powershell
# Exemplo: criar tópico com 3 partições
docker exec auction-kafka kafka-topics --create `
  --topic bid-placed `
  --partitions 3 `
  --replication-factor 1 `
  --bootstrap-server localhost:9092
```

---

## 📝 Troubleshooting

### Porta já em uso
```powershell
# Verificar processos usando portas
netstat -ano | findstr "5432"
netstat -ano | findstr "6379"
netstat -ano | findstr "29092"

# Matar processo (substitua PID)
taskkill /F /PID <PID>
```

### Container não inicia / unhealthy
```powershell
# Ver logs detalhados
docker-compose logs <service-name>

# Ver status de saúde
docker inspect auction-postgres --format='{{.State.Health.Status}}'

# Restart específico
docker-compose restart <service-name>
```

### Migrations não aplicam
```powershell
# Verificar connection string no appsettings.json
# Verificar se PostgreSQL está rodando
docker-compose ps postgres

# Tentar aplicar com verbose
dotnet ef database update --project Auction.Infrastructure --verbose
```

### Redis connection refused
```powershell
# Verificar se Redis está aceitando conexões
docker exec -it auction-redis redis-cli -a redis123 PING

# Reiniciar Redis
docker-compose restart redis
```

---

## 🎯 Quick Start (Copiar e Colar)

```powershell
# Iniciar tudo e aplicar migrations
docker-compose up -d && `
Start-Sleep -Seconds 30 && `
dotnet ef database update --project Auction.Infrastructure && `
Write-Host "✅ Infraestrutura pronta!" -ForegroundColor Green
```
