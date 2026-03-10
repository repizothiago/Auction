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
public class CancelAuctionCommandHandler : ICommandHandler<CancelAuctionCommand>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelAuctionCommandHandler> _logger;

    public CancelAuctionCommandHandler(
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelAuctionCommandHandler> logger)
    {
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(
        CancelAuctionCommand command, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing CancelAuctionCommand for AuctionId: {AuctionId}, Reason: {Reason}",
            command.AuctionId,
            command.Reason);

        // Buscar leilão
        var auction = await _auctionRepository.GetByIdAsync(command.AuctionId, cancellationToken);

        if (auction is null)
        {
            _logger.LogWarning("Auction {AuctionId} not found", command.AuctionId);
            return Result.Failure(new Error(
                "Auction.NotFound", 
                $"Leilão com ID {command.AuctionId} não foi encontrado"));
        }

        // Cancelar leilão (domain logic)
        var cancelResult = auction.Cancel(command.Reason);

        if (!cancelResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to cancel auction {AuctionId}: {Error}",
                command.AuctionId,
                cancelResult.Error?.Message);
            return cancelResult;
        }

        // Atualizar no repositório
        _auctionRepository.Update(auction);

        // Salvar mudanças (isso vai disparar Domain Events automaticamente)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Auction {AuctionId} cancelled successfully. Reason: {Reason}",
            command.AuctionId,
            command.Reason);

        return Result.Success();
    }
}
