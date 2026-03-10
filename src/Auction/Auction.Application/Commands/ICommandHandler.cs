using Auction.SharedKernel;

namespace Auction.Application.Commands;

/// <summary>
/// Interface base para Command Handlers (CQRS)
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> 
    where TCommand : class
{
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface para Command Handlers sem retorno (void)
/// </summary>
public interface ICommandHandler<in TCommand> 
    where TCommand : class
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
