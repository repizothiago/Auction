using Auction.SharedKernel;

namespace Auction.Application.Commands;

/// <summary>
/// Interface para Command Handlers sem retorno (void)
/// </summary>
public interface ICommandHandler<in TCommand>
    where TCommand : class
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
