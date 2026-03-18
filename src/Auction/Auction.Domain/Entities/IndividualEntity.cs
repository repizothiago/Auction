using Auction.Domain.Events.User;
using Auction.Domain.ValueObjects;
using Auction.SharedKernel;

namespace Auction.Domain.Entities;

public class IndividualEntity : User
{
    protected IndividualEntity() { }

    private IndividualEntity(Email email, Cpf cpf, Password password) : base(email, password)
    {
        Cpf = cpf;
    }

    public Cpf Cpf { get; private set; }


    public static Result<IndividualEntity> Create(string emailString, string cpfString, string passwordString)
    {
        var emailResult = Email.Create(emailString);
        if (!emailResult.IsSuccess)
            return Result<IndividualEntity>.Failure(emailResult.Error);

        var cpfResult = Cpf.Create(cpfString);
        if (!cpfResult.IsSuccess)
            return Result<IndividualEntity>.Failure(cpfResult.Error);

        var passwordResult = Password.Create(passwordString);
        if (!passwordResult.IsSuccess)
            return Result<IndividualEntity>.Failure(passwordResult.Error);

        var user = new IndividualEntity(emailResult.Value, cpfResult.Value, passwordResult.Value);

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value, user.Cpf.Value));

        return Result<IndividualEntity>.Success(user);
    }
}