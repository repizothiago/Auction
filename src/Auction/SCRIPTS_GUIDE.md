# 📜 Scripts Disponíveis - Sistema de Leilão

## 🎯 Overview

Este documento descreve todos os scripts automatizados disponíveis para facilitar o setup e gerenciamento da infraestrutura.

---

## 1. 📥 `download-docker-images` - Download de Imagens

**Propósito:** Baixar todas as imagens Docker necessárias antes de iniciar os containers.

**Arquivos:**
- `download-docker-images.ps1` (Windows PowerShell)
- `download-docker-images.sh` (Linux/Mac Bash)

**O que faz:**
- ✅ Verifica se Docker está instalado e rodando
- ✅ Baixa 7 imagens Docker (~2.4GB total)
- ✅ Mostra progresso detalhado de cada download
- ✅ Identifica imagens que já existem localmente
- ✅ Verifica atualizações disponíveis
- ✅ Relatório de sucesso/falha ao final
- ✅ Mostra espaço em disco utilizado

**Quando usar:**
- ⭐ **Primeira vez** configurando o ambiente
- 🔄 Periodicamente para atualizar imagens
- 🌐 Em conexões lentas (download em background)

**Como executar:**

### Windows
```powershell
./download-docker-images.ps1
```

### Linux/Mac
```bash
chmod +x download-docker-images.sh
./download-docker-images.sh
```

**Imagens baixadas:**
1. `postgres:16-alpine` (~84MB)
2. `redis:7-alpine` (~32MB)
3. `confluentinc/cp-zookeeper:7.5.0` (~710MB)
4. `confluentinc/cp-kafka:7.5.0` (~770MB)
5. `provectuslabs/kafka-ui:latest` (~200MB)
6. `dpage/pgadmin4:latest` (~470MB)
7. `rediscommander/redis-commander:latest` (~120MB)

**Output esperado:**
```
🐳 Script de Download de Imagens Docker
============================================================

📦 Verificando Docker...
✓ Docker encontrado: Docker version 24.0.x
✓ Docker está rodando

📋 Imagens a serem baixadas: 7
   Tamanho total estimado: ~2.4GB

[1/7] PostgreSQL - Banco de dados principal
   Imagem: postgres:16-alpine
   Tamanho: ~84MB
   ✓ Download concluído!

[2/7] Redis - Cache distribuído
   ...

============================================================
📊 RESUMO DO DOWNLOAD
============================================================

✅ Imagens baixadas com sucesso: 7/7

🎉 TODAS AS IMAGENS BAIXADAS COM SUCESSO!
```

---

## 2. 🚀 `start-infrastructure` - Inicialização Completa

**Propósito:** Iniciar toda a infraestrutura (Docker + Migrations) de forma automatizada.

**Arquivos:**
- `start-infrastructure.ps1` (Windows PowerShell)

**O que faz:**
- ✅ Verifica se Docker está instalado e rodando
- ✅ Executa `docker-compose up -d`
- ✅ Aguarda containers ficarem healthy (30s)
- ✅ Aplica migrations do EF Core
- ✅ Mostra status de todos os serviços
- ✅ Exibe resumo com URLs de acesso
- ✅ Próximos passos sugeridos

**Quando usar:**
- ⭐ **Primeira vez** após baixar as imagens
- 🔄 Toda vez que precisar iniciar o ambiente
- 🛠️ Após resetar o ambiente (`docker-compose down`)

**Como executar:**

### Windows
```powershell
./start-infrastructure.ps1
```

**Output esperado:**
```
🚀 Iniciando infraestrutura do Sistema de Leilão...

📦 Verificando Docker...
✓ Docker encontrado: Docker version 24.0.x
✓ Docker está rodando

🐳 Iniciando containers Docker...
✓ Containers iniciados com sucesso!

⏳ Aguardando containers ficarem prontos (30 segundos)...

📊 Status dos containers:
NAME                  STATUS         PORTS
auction-postgres      Up             0.0.0.0:5432->5432/tcp
auction-redis         Up             0.0.0.0:6379->6379/tcp
auction-kafka         Up             0.0.0.0:29092->29092/tcp
...

🗄️  Aplicando migrations ao banco de dados...
✓ Migrations aplicadas com sucesso!

============================================================
✅ INFRAESTRUTURA PRONTA!
============================================================

📋 Serviços disponíveis:
   • PostgreSQL      : localhost:5432
   • Redis           : localhost:6379
   • Kafka           : localhost:29092
   • pgAdmin         : http://localhost:5050
   • Kafka UI        : http://localhost:8080
   • Redis Commander : http://localhost:8081

🎯 Próximos passos:
   1. Execute a API: dotnet run --project Auction.Api
   2. Acesse pgAdmin: http://localhost:5050
   3. Acesse Kafka UI: http://localhost:8080
```

---

## 📊 Comparação dos Scripts

| Feature | download-docker-images | start-infrastructure |
|---------|----------------------|---------------------|
| **Download de imagens** | ✅ Sim | ❌ Não (usa cache) |
| **Inicia containers** | ❌ Não | ✅ Sim |
| **Aplica migrations** | ❌ Não | ✅ Sim |
| **Verifica Docker** | ✅ Sim | ✅ Sim |
| **Mostra progresso** | ✅ Detalhado | ✅ Básico |
| **Quando usar** | Antes de tudo | Após download |

---

## 🎯 Fluxo Recomendado

### Primeira Vez (Setup Inicial)
```powershell
# 1. Baixar imagens (uma vez só)
./download-docker-images.ps1

# 2. Iniciar infraestrutura
./start-infrastructure.ps1

# 3. Pronto! 🎉
```

### Uso Diário
```powershell
# Se containers estiverem parados
./start-infrastructure.ps1

# Ou manualmente
docker-compose up -d
```

### Atualizar Imagens
```powershell
# 1. Parar containers
docker-compose down

# 2. Atualizar imagens
./download-docker-images.ps1

# 3. Reiniciar
./start-infrastructure.ps1
```

---

## 🛠️ Troubleshooting

### Script não executa (PowerShell)
```powershell
# Verificar política de execução
Get-ExecutionPolicy

# Permitir scripts (se necessário)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Script não executa (Bash)
```bash
# Dar permissão de execução
chmod +x download-docker-images.sh
chmod +x start-infrastructure.sh
```

### Docker não encontrado
1. Instale Docker Desktop
2. Reinicie o terminal
3. Execute novamente

### Download falha
1. Verifique conexão com internet
2. Verifique se Docker Hub está acessível
3. Tente baixar imagens manualmente:
   ```bash
   docker pull postgres:16-alpine
   ```

---

## 💡 Dicas

### 1. Download em Background
```powershell
# Windows - PowerShell em nova janela
Start-Process powershell -ArgumentList "-File download-docker-images.ps1"

# Linux/Mac - Background com nohup
nohup ./download-docker-images.sh &
```

### 2. Ver logs durante inicialização
```powershell
# Em outro terminal
docker-compose logs -f
```

### 3. Verificar saúde dos containers
```powershell
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

---

## 📚 Documentação Relacionada

- **[DOCKER_SETUP.md](DOCKER_SETUP.md)** - Guia completo de Docker
- **[README.md](Auction.Infrastructure/README.md)** - Documentação da Infrastructure
- **[MIGRATIONS_GUIDE.md](Auction.Infrastructure/Persistence/MIGRATIONS_GUIDE.md)** - Guia de migrations

---

## 🔄 Atualização dos Scripts

Os scripts são mantidos no repositório. Para obter a versão mais recente:

```bash
git pull origin dev
```

---

**Última atualização:** 2025
**Versão:** 1.0
