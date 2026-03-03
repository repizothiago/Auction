# ============================================
# Script de Download de Imagens Docker
# Sistema de Leilao
# ============================================

Write-Host "`n SCRIPT DE DOWNLOAD DE IMAGENS DOCKER" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# Verificar se Docker esta instalado
Write-Host "`nVerificando Docker..." -ForegroundColor Yellow

try {
    $dockerVersion = docker --version
    Write-Host "Docker encontrado: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "Docker nao encontrado. Por favor, instale o Docker Desktop." -ForegroundColor Red
    Write-Host "Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Verificar se Docker esta rodando
try {
    docker ps | Out-Null
    Write-Host "Docker esta rodando" -ForegroundColor Green
} catch {
    Write-Host "Docker nao esta rodando. Por favor, inicie o Docker Desktop." -ForegroundColor Yellow
    exit 1
}

# Lista de imagens necessarias
$images = @(
    @{Name="postgres:16-alpine"; Size="~84MB"; Description="PostgreSQL - Banco de dados principal"},
    @{Name="redis:7-alpine"; Size="~32MB"; Description="Redis - Cache distribuido"},
    @{Name="confluentinc/cp-zookeeper:7.5.0"; Size="~710MB"; Description="Zookeeper - Coordenacao Kafka"},
    @{Name="confluentinc/cp-kafka:7.5.0"; Size="~770MB"; Description="Kafka - Message Broker"},
    @{Name="provectuslabs/kafka-ui:latest"; Size="~200MB"; Description="Kafka UI - Interface web"},
    @{Name="dpage/pgadmin4:latest"; Size="~470MB"; Description="pgAdmin - Interface PostgreSQL"},
    @{Name="rediscommander/redis-commander:latest"; Size="~120MB"; Description="Redis Commander - Interface Redis"}
)

$totalImages = $images.Count
$successCount = 0
$failedImages = @()

Write-Host "`nImagens a serem baixadas: $totalImages" -ForegroundColor White
Write-Host "Tamanho total estimado: ~2.4GB" -ForegroundColor Gray
Write-Host ""

# Loop atraves das imagens
$currentImage = 0
foreach ($image in $images) {
    $currentImage++
    
    Write-Host "`n[$currentImage/$totalImages] " -NoNewline -ForegroundColor Cyan
    Write-Host $image.Description -ForegroundColor White
    Write-Host "Imagem: " -NoNewline -ForegroundColor Gray
    Write-Host $image.Name -ForegroundColor Yellow
    Write-Host "Tamanho: " -NoNewline -ForegroundColor Gray
    Write-Host $image.Size -ForegroundColor Yellow
    
    # Verificar se imagem ja existe
    $existingImage = docker images -q $image.Name 2>$null
    
    if ($existingImage) {
        Write-Host "Imagem ja existe localmente" -ForegroundColor Blue
        Write-Host "Verificando atualizacoes..." -ForegroundColor Yellow
    } else {
        Write-Host "Baixando imagem..." -ForegroundColor Yellow
    }
    
    # Fazer pull da imagem
    try {
        $pullOutput = docker pull $image.Name 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Download concluido!" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "Falha no download" -ForegroundColor Red
            $failedImages += $image.Name
        }
    } catch {
        Write-Host "Erro: $_" -ForegroundColor Red
        $failedImages += $image.Name
    }
}

# Resumo final
Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "RESUMO DO DOWNLOAD" -ForegroundColor White
Write-Host "============================================================" -ForegroundColor Cyan

Write-Host "`nImagens baixadas com sucesso: " -NoNewline -ForegroundColor Green
Write-Host "$successCount/$totalImages" -ForegroundColor White

if ($failedImages.Count -gt 0) {
    Write-Host "Imagens com falha: " -NoNewline -ForegroundColor Red
    Write-Host $failedImages.Count -ForegroundColor White
    Write-Host "`nImagens que falharam:" -ForegroundColor Yellow
    foreach ($failed in $failedImages) {
        Write-Host "  - $failed" -ForegroundColor Gray
    }
}

# Listar todas as imagens baixadas
Write-Host "`nImagens Docker disponiveis:" -ForegroundColor White
Write-Host ""
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}" | Select-String -Pattern "postgres|redis|kafka|zookeeper|pgadmin|provectuslabs"

# Verificar espaco em disco
Write-Host "`nEspaco utilizado pelo Docker:" -ForegroundColor White
$dockerDiskUsage = docker system df --format "table {{.Type}}\t{{.Size}}"
Write-Host $dockerDiskUsage -ForegroundColor Gray

# Instrucoes finais
if ($successCount -eq $totalImages) {
    Write-Host "`n============================================================" -ForegroundColor Green
    Write-Host "TODAS AS IMAGENS BAIXADAS COM SUCESSO!" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
    
    Write-Host "`nProximos passos:" -ForegroundColor White
    Write-Host "1. Execute: docker-compose up -d" -ForegroundColor Cyan
    Write-Host "2. Ou use: ./start-infrastructure.ps1" -ForegroundColor Cyan
    
} else {
    Write-Host "`nAlgumas imagens falharam ao baixar." -ForegroundColor Yellow
    Write-Host "Verifique sua conexao com a internet e tente novamente." -ForegroundColor Gray
    Write-Host "`nPara tentar novamente apenas as imagens que falharam:" -ForegroundColor White
    foreach ($failed in $failedImages) {
        Write-Host "docker pull $failed" -ForegroundColor Cyan
    }
}

Write-Host "`nComandos uteis:" -ForegroundColor White
Write-Host "- Listar imagens       : docker images" -ForegroundColor Gray
Write-Host "- Remover imagem       : docker rmi NOME_DA_IMAGEM" -ForegroundColor Gray
Write-Host "- Remover imagens orfas: docker image prune" -ForegroundColor Gray
Write-Host "- Espaco em disco      : docker system df" -ForegroundColor Gray

Write-Host ""
