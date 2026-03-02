using Auction.Domain.Events;
using Auction.Domain.ValueObjects;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.Entities;

public class CorporateUser : User
{
    protected CorporateUser() { }

    private CorporateUser(Email email, Cnpj cnpj, Password password, string companyName) : base(email, password)
    {
        Cnpj = cnpj;
        CompanyName = companyName;
    }

    public Cnpj Cnpj { get; private set; }
    public string CompanyName { get; private set; }

    public static Result<CorporateUser> Create(string emailString, string cnpjString, string passwordString, string companyName)
    {
        var emailResult = Email.Create(emailString);
        if (!emailResult.IsSuccess)
            return Result<CorporateUser>.Failure(emailResult.Error);

        var cnpjResult = Cnpj.Create(cnpjString);
        if (!cnpjResult.IsSuccess)
            return Result<CorporateUser>.Failure(cnpjResult.Error);

        var passwordResult = Password.Create(passwordString);
        if (!passwordResult.IsSuccess)
            return Result<CorporateUser>.Failure(passwordResult.Error);

        if (string.IsNullOrWhiteSpace(companyName))
            return Result<CorporateUser>.Failure(new Error("CorporateUser.InvalidCompanyName", "Razão social é obrigatória."));

        var user = new CorporateUser(emailResult.Value, cnpjResult.Value, passwordResult.Value, companyName);
        
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value, user.Cnpj.Value));

        return Result<CorporateUser>.Success(user);
    }
}