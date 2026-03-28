using Auction.Application.Commands;
using Auction.Application.Commands.Auction;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace Auction.Application.CommandHandlers.Auction;

/// <summary>
/// Handler responsável por processar o comando de cancelamento de leilão
/// </summary>
public class CancelAuctionCommandHandler(IAuctionRepository auctionRepository,
                                   IUnitOfWork unitOfWork,
                                   ILogger<CancelAuctionCommandHandler> logger) : ICommandHandler<CancelAuctionCommand>
{
    private readonly IAuctionRepository _auctionRepository = auctionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CancelAuctionCommandHandler> _logger = logger;

    public async Task<Result> HandleAsync(
        CancelAuctionCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Comando] Processando cancelamento de leilão: AuctionId={AuctionId}, Motivo={Motivo}",
            command.AuctionId,
            command.Reason);

        // Buscar leilão
        var auction = await _auctionRepository.GetByIdAsync(command.AuctionId, cancellationToken);

        if (auction is null)
        {
            _logger.LogWarning("[Comando] Leilão não encontrado: AuctionId={AuctionId}", command.AuctionId);
            return Result.Failure(Error.NotFound(
                "Auction.NotFound",
                $"Leilão com ID {command.AuctionId} não foi encontrado"));
        }

        // Cancelar leilão (domain logic)
        var cancelResult = auction.Cancel(command.Reason);

        if (!cancelResult.IsSuccess)
        {
            _logger.LogWarning(
                "[Comando] Falha ao cancelar leilão: AuctionId={AuctionId}, Erro={Erro}",
                command.AuctionId,
                cancelResult.Error?.Message);
            return cancelResult;
        }

        // Atualizar no repositório
        _auctionRepository.Update(auction);

        // Salvar mudanças (isso vai disparar Domain Events automaticamente)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[Comando] Leilão cancelado com sucesso: AuctionId={AuctionId}, Motivo={Motivo}",
            command.AuctionId,
            command.Reason);

        return Result.Success();
    }
}
