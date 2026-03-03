# ✅ Setup Checklist - Sistema de Leilão

Use este checklist para configurar o ambiente de desenvolvimento pela primeira vez.

---

## 📋 Pré-requisitos

- [ ] **.NET 10 SDK** instalado
  - Verificar: `dotnet --version`
  - Download: https://dotnet.microsoft.com/download/dotnet/10.0

- [ ] **Docker Desktop** instalado e rodando
  - Verificar: `docker --version`
  - Verificar: `docker ps`
  - Download: https://www.docker.com/products/docker-desktop

- [ ] **Git** instalado
  - Verificar: `git --version`
  - Download: https://git-scm.com/

- [ ] **EF Core Tools** instalado
  - Verificar: `dotnet ef --version`
  - Instalar: `dotnet tool install --global dotnet-ef`

---

## 🔧 Configuração Inicial

### 1. Clone do Repositório
- [ ] Clone do repositório
  ```bash
  git clone https://github.com/repizothiago/Auction.git
  cd Auction/src/Auction
  ```

### 2. PowerShell (apenas Windows)
- [ ] Verificar política de execução
  ```powershell
  Get-ExecutionPolicy
  ```
- [ ] Se necessário, permitir scripts:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

### 3. Download de Imagens Docker
- [ ] Executar script de download
  ```powershell
  # Windows
  ./download-docker-images.ps1
  
  # Linux/Mac
  chmod +x download-docker-images.sh
  ./download-docker-images.sh
  ```
- [ ] Verificar se todas as 7 imagens foram baixadas com sucesso
- [ ] Tamanho total: ~2.4GB

### 4. Configuração (Opcional)
- [ ] Revisar `appsettings.json` em `Auction.Api`
- [ ] Verificar connection strings
  - PostgreSQL: `localhost:5432`
  - Redis: `localhost:6379`
  - Kafka: `localhost:29092`

---

## 🚀 Inicialização

### 5. Infraestrutura
- [ ] Executar script de inicialização
  ```powershell
  ./start-infrastructure.ps1
  ```
- [ ] Verificar containers rodando
  ```powershell
  docker-compose ps
  ```
- [ ] Todos os containers devem estar "Up" e "healthy"

### 6. Migrations
- [ ] Migrations aplicadas automaticamente pelo script
- [ ] Verificar migrations aplicadas:
  ```powershell
  dotnet ef migrations list --project Auction.Infrastructure
  ```
- [ ] Migration `InitialCreate` deve estar marcada como aplicada

### 7. Verificação da Infraestrutura
- [ ] **PostgreSQL** respondendo
  ```powershell
  docker exec -it auction-postgres psql -U postgres -d auction_db -c "\dt"
  ```
- [ ] **Redis** respondendo
  ```powershell
  docker exec -it auction-redis redis-cli -a redis123 PING
  ```
- [ ] **Kafka** respondendo
  ```powershell
  docker exec -it auction-kafka kafka-topics --bootstrap-server localhost:9092 --list
  ```

### 8. Interfaces Web
- [ ] **pgAdmin** acessível: http://localhost:5050
  - Email: `admin@auction.com`
  - Password: `admin`
  - Conectar ao servidor:
    - Host: `postgres` (ou `host.docker.internal`)
    - Port: `5432`
    - User: `postgres`
    - Password: `postgres`
    - Database: `auction_db`

- [ ] **Kafka UI** acessível: http://localhost:8080

- [ ] **Redis Commander** acessível: http://localhost:8081

---

## 🎯 Executar Aplicação

### 9. Build e Execução
- [ ] Restaurar pacotes
  ```powershell
  dotnet restore
  ```

- [ ] Build da solution
  ```powershell
  dotnet build
  ```

- [ ] Executar a API
  ```powershell
  dotnet run --project Auction.Api
  ```

- [ ] **API** acessível:
  - HTTPS: https://localhost:7001
  - HTTP: http://localhost:5000
  - Swagger: https://localhost:7001/swagger

---

## ✅ Verificação Final

### 10. Health Checks
- [ ] Testar health endpoint (quando implementado)
  ```
  GET https://localhost:7001/health
  ```

### 11. Database
- [ ] Tabelas criadas no PostgreSQL:
  - [ ] `users`
  - [ ] `categories` (com 7 registros)
  - [ ] `auctions`

- [ ] Verificar no pgAdmin:
  ```sql
  SELECT * FROM categories;
  SELECT COUNT(*) FROM categories; -- Deve retornar 7
  ```

### 12. Cache
- [ ] Redis funcionando
  ```powershell
  docker exec -it auction-redis redis-cli -a redis123
  > PING
  PONG
  > KEYS *
  (empty array)
  ```

### 13. Messaging
- [ ] Kafka topics criados (ou configurado para auto-create)
  ```powershell
  docker exec auction-kafka kafka-topics --list --bootstrap-server localhost:9092
  ```

---

## 📚 Próximos Passos

Após completar este checklist:

- [ ] Ler [AUCTION_SYSTEM_ARCHITECTURE.md](AUCTION_SYSTEM_ARCHITECTURE.md)
- [ ] Explorar código do Domain Layer
- [ ] Revisar Entity Configurations
- [ ] Testar endpoints da API (quando implementados)
- [ ] Configurar IDE (VS Code ou Visual Studio)
- [ ] Configurar debugging

---

## 🐛 Problemas Comuns

### Docker não inicia
**Causa:** Docker Desktop não está rodando  
**Solução:** Iniciar Docker Desktop manualmente

### Porta já em uso
**Causa:** Outro processo usando a porta  
**Solução:**
```powershell
# Encontrar processo
netstat -ano | findstr "5432"
# Matar processo
taskkill /F /PID <PID>
```

### Migrations falham
**Causa:** PostgreSQL não está pronto  
**Solução:** Aguardar 30 segundos após `docker-compose up` antes de aplicar migrations

### Script não executa (PowerShell)
**Causa:** Política de execução restritiva  
**Solução:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Imagens não baixam
**Causa:** Problema de conexão ou Docker Hub  
**Solução:** Tentar novamente ou baixar manualmente:
```powershell
docker pull postgres:16-alpine
```

---

## 📞 Precisa de Ajuda?

- 📖 **Documentação**: Ver [README.md](README.md)
- 🐛 **Issues**: [GitHub Issues](https://github.com/repizothiago/Auction/issues)
- 💬 **Discussões**: [GitHub Discussions](https://github.com/repizothiago/Auction/discussions)

---

## 🎉 Parabéns!

Se você completou todos os itens deste checklist, seu ambiente está configurado e pronto para desenvolvimento! 

**Próximos passos sugeridos:**
1. Explorar o código
2. Implementar novos features
3. Escrever testes
4. Contribuir com o projeto

---

**Última atualização:** 2025  
**Versão do Checklist:** 1.0
