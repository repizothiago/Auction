namespace Auction.Application.Commands;

/// <summary>
/// Marker interface para Commands (CQRS pattern)
/// </summary>
public interface ICommand<TResponse> where TResponse : class
{
}

/// <summary>
/// Marker interface para Commands sem retorno
/// </summary>
public interface ICommand
{
}
