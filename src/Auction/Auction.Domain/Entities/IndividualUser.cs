using Auction.Domain.Events;
using Auction.Domain.ValueObjects;
using Auction.SharedKernel;

namespace Auction.Domain.Entities;

public class IndividualUser : User
{
    protected IndividualUser() { }

    private IndividualUser(Email email, Cpf cpf, Password password) : base(email, password)
    {
        Cpf = cpf;
    }

    public Cpf Cpf { get; private set; }

    public static Result<IndividualUser> Create(string emailString, string cpfString, string passwordString)
    {
        var emailResult = Email.Create(emailString);
        if (!emailResult.IsSuccess)
            return Result<IndividualUser>.Failure(emailResult.Error);

        var cpfResult = Cpf.Create(cpfString);
        if (!cpfResult.IsSuccess)
            return Result<IndividualUser>.Failure(cpfResult.Error);

        var passwordResult = Password.Create(passwordString);
        if (!passwordResult.IsSuccess)
            return Result<IndividualUser>.Failure(passwordResult.Error);

        var user = new IndividualUser(emailResult.Value, cpfResult.Value, passwordResult.Value);
        
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value, user.Cpf.Value));

        return Result<IndividualUser>.Success(user);
    }
}