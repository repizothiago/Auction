#!/bin/bash

# ============================================
# Script de Download de Imagens Docker
# Sistema de Leilão
# ============================================

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "\n${CYAN}🐳 Script de Download de Imagens Docker${NC}"
echo "============================================================"

# Verificar se Docker está instalado
echo -e "\n${YELLOW}📦 Verificando Docker...${NC}"

if ! command -v docker &> /dev/null; then
    echo -e "${RED}✗ Docker não encontrado. Por favor, instale o Docker.${NC}"
    echo -e "${YELLOW}   Download: https://docs.docker.com/get-docker/${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker encontrado: $(docker --version)${NC}"

# Verificar se Docker está rodando
if ! docker ps &> /dev/null; then
    echo -e "${RED}✗ Docker não está rodando. Por favor, inicie o Docker.${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker está rodando${NC}"

# Lista de imagens necessárias
declare -a images=(
    "postgres:16-alpine|~84MB|PostgreSQL - Banco de dados principal"
    "redis:7-alpine|~32MB|Redis - Cache distribuído"
    "confluentinc/cp-zookeeper:7.5.0|~710MB|Zookeeper - Coordenação Kafka"
    "confluentinc/cp-kafka:7.5.0|~770MB|Kafka - Message Broker"
    "provectuslabs/kafka-ui:latest|~200MB|Kafka UI - Interface web"
    "dpage/pgadmin4:latest|~470MB|pgAdmin - Interface PostgreSQL"
    "rediscommander/redis-commander:latest|~120MB|Redis Commander - Interface Redis"
)

total_images=${#images[@]}
success_count=0
declare -a failed_images=()

echo -e "\n📋 Imagens a serem baixadas: $total_images"
echo -e "   Tamanho total estimado: ~2.4GB"
echo ""

# Loop através das imagens
current_image=0
for image_info in "${images[@]}"; do
    ((current_image++))
    
    IFS='|' read -r image_name image_size image_desc <<< "$image_info"
    
    echo -e "\n${CYAN}[$current_image/$total_images]${NC} ${image_desc}"
    echo -e "   Imagem: ${YELLOW}${image_name}${NC}"
    echo -e "   Tamanho: ${YELLOW}${image_size}${NC}"
    
    # Verificar se imagem já existe
    if docker images -q "$image_name" &> /dev/null && [ -n "$(docker images -q $image_name)" ]; then
        echo -e "   ${BLUE}ℹ️  Imagem já existe localmente${NC}"
        echo -e "   ${YELLOW}🔄 Verificando atualizações...${NC}"
    else
        echo -e "   ${YELLOW}📥 Baixando imagem...${NC}"
    fi
    
    # Fazer pull da imagem
    if docker pull "$image_name" &> /dev/null; then
        echo -e "   ${GREEN}✓ Download concluído!${NC}"
        ((success_count++))
    else
        echo -e "   ${RED}✗ Falha no download${NC}"
        failed_images+=("$image_name")
    fi
done

# Resumo final
echo -e "\n============================================================"
echo -e "${NC}📊 RESUMO DO DOWNLOAD${NC}"
echo "============================================================"

echo -e "\n${GREEN}✅ Imagens baixadas com sucesso: $success_count/$total_images${NC}"

if [ ${#failed_images[@]} -gt 0 ]; then
    echo -e "${RED}❌ Imagens com falha: ${#failed_images[@]}${NC}"
    echo -e "\n   ${YELLOW}Imagens que falharam:${NC}"
    for failed in "${failed_images[@]}"; do
        echo -e "   ${NC}• $failed${NC}"
    done
fi

# Listar todas as imagens baixadas
echo -e "\n📦 Imagens Docker disponíveis:"
echo ""
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}" | grep -E "postgres|redis|kafka|zookeeper|pgadmin|provectuslabs"

# Verificar espaço em disco
echo -e "\n💾 Espaço utilizado pelo Docker:"
docker system df

# Instruções finais
if [ $success_count -eq $total_images ]; then
    echo -e "\n============================================================"
    echo -e "${GREEN}🎉 TODAS AS IMAGENS BAIXADAS COM SUCESSO!${NC}"
    echo "============================================================"
    
    echo -e "\n🚀 Próximos passos:"
    echo -e "   1. Execute: ${CYAN}docker-compose up -d${NC}"
    echo -e "   2. Ou use: ${CYAN}./start-infrastructure.sh${NC}"
else
    echo -e "\n${YELLOW}⚠️  Algumas imagens falharam ao baixar.${NC}"
    echo -e "   Verifique sua conexão com a internet e tente novamente."
    echo -e "\n   Para tentar novamente apenas as imagens que falharam:"
    for failed in "${failed_images[@]}"; do
        echo -e "   ${CYAN}docker pull $failed${NC}"
    done
fi

echo -e "\n💡 Comandos úteis:"
echo -e "   • Listar imagens       : ${CYAN}docker images${NC}"
echo -e "   • Remover imagem       : ${CYAN}docker rmi <image>${NC}"
echo -e "   • Remover imagens órfãs: ${CYAN}docker image prune${NC}"
echo -e "   • Espaço em disco      : ${CYAN}docker system df${NC}"

echo ""
