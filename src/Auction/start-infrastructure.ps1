# ============================================
# Script de Inicialização da Infraestrutura
# Sistema de Leilão
# ============================================

Write-Host "`n🚀 Iniciando infraestrutura do Sistema de Leilão..." -ForegroundColor Cyan

# Verificar se Docker está instalado e rodando
Write-Host "`n📦 Verificando Docker..." -ForegroundColor Yellow

try {
    $dockerVersion = docker --version
    Write-Host "✓ Docker encontrado: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker não encontrado. Por favor, instale o Docker Desktop." -ForegroundColor Red
    Write-Host "   Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Verificar se Docker está rodando
try {
    docker ps | Out-Null
    Write-Host "✓ Docker está rodando" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker não está rodando. Iniciando Docker Desktop..." -ForegroundColor Yellow
    Write-Host "   Por favor, inicie o Docker Desktop e execute este script novamente." -ForegroundColor Yellow
    exit 1
}

# Navegar para o diretório do docker-compose
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "`n🐳 Iniciando containers Docker..." -ForegroundColor Yellow

# Subir containers
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Containers iniciados com sucesso!" -ForegroundColor Green
} else {
    Write-Host "✗ Erro ao iniciar containers" -ForegroundColor Red
    exit 1
}

# Aguardar containers ficarem healthy
Write-Host "`n⏳ Aguardando containers ficarem prontos (30 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar status dos containers
Write-Host "`n📊 Status dos containers:" -ForegroundColor Yellow
docker-compose ps

# Aplicar migrations
Write-Host "`n🗄️  Aplicando migrations ao banco de dados..." -ForegroundColor Yellow

try {
    dotnet ef database update --project Auction.Infrastructure
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Migrations aplicadas com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "✗ Erro ao aplicar migrations" -ForegroundColor Red
        Write-Host "   Execute manualmente: dotnet ef database update --project Auction.Infrastructure" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Erro ao aplicar migrations: $_" -ForegroundColor Red
    Write-Host "   Execute manualmente: dotnet ef database update --project Auction.Infrastructure" -ForegroundColor Yellow
}

# Resumo final
Write-Host "`n" + "="*60 -ForegroundColor Cyan
Write-Host "✅ INFRAESTRUTURA PRONTA!" -ForegroundColor Green
Write-Host "="*60 -ForegroundColor Cyan

Write-Host "`n📋 Serviços disponíveis:" -ForegroundColor White
Write-Host "   • PostgreSQL      : localhost:5432 (user: postgres, pass: postgres)" -ForegroundColor Gray
Write-Host "   • Redis           : localhost:6379 (pass: redis123)" -ForegroundColor Gray
Write-Host "   • Kafka           : localhost:29092" -ForegroundColor Gray
Write-Host "   • pgAdmin         : http://localhost:5050" -ForegroundColor Gray
Write-Host "   • Kafka UI        : http://localhost:8080" -ForegroundColor Gray
Write-Host "   • Redis Commander : http://localhost:8081" -ForegroundColor Gray

Write-Host "`n📚 Para mais informações, consulte:" -ForegroundColor White
Write-Host "   • DOCKER_SETUP.md                     - Guia completo" -ForegroundColor Gray
Write-Host "   • Auction.Infrastructure/Persistence/MIGRATIONS_GUIDE.md - Guia de migrations" -ForegroundColor Gray

Write-Host "`n🎯 Próximos passos:" -ForegroundColor White
Write-Host "   1. Execute a API: dotnet run --project Auction.Api" -ForegroundColor Gray
Write-Host "   2. Acesse pgAdmin para visualizar o banco: http://localhost:5050" -ForegroundColor Gray
Write-Host "   3. Acesse Kafka UI para monitorar tópicos: http://localhost:8080" -ForegroundColor Gray

Write-Host "`n💡 Comandos úteis:" -ForegroundColor White
Write-Host "   • Ver logs        : docker-compose logs -f" -ForegroundColor Gray
Write-Host "   • Parar serviços  : docker-compose stop" -ForegroundColor Gray
Write-Host "   • Remover tudo    : docker-compose down -v" -ForegroundColor Gray

Write-Host ""
