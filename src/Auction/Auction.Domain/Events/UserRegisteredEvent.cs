using Auction.SharedKernel;

namespace Auction.Domain.Events;

public sealed record UserRegisteredEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string Cpf { get; }

    public UserRegisteredEvent(Guid userId, string email, string cpf)
    {
        UserId = userId;
        Email = email;
        Cpf = cpf;
    }
}
