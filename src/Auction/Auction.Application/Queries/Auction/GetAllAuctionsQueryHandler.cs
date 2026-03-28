using Auction.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Auction.Application.Queries.Auction;

/// <summary>
/// Handler para processar a query GetAllAuctions
/// </summary>
public class GetAllAuctionsQueryHandler
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ILogger<GetAllAuctionsQueryHandler> _logger;

    public GetAllAuctionsQueryHandler(
        IAuctionRepository auctionRepository,
        ILogger<GetAllAuctionsQueryHandler> logger)
    {
        _auctionRepository = auctionRepository;
        _logger = logger;
    }

    public async Task<List<Domain.Entities.Auction>> HandleAsync(
        GetAllAuctionsQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Consulta] Buscando todos os leilões: Pagina={Pagina}, TamanhoPagina={TamanhoPagina}",
            query.PageNumber,
            query.PageSize);

        var auctions = await _auctionRepository.GetAllAsync(
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        _logger.LogInformation("[Consulta] Leilões encontrados: Total={Total}", auctions.Count);

        return auctions;
    }
}
