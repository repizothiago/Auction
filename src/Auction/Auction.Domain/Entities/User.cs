using Auction.Domain.Entities.Base;
using Auction.Domain.ValueObjects;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;

namespace Auction.Domain.Entities;

public abstract class User : BaseEntity
{
    protected User() { }

    protected User(Email email, Password password) : base(Guid.NewGuid(), DateTime.Now, null)
    {
        Email = email;
        Password = password;
    }

    public Email Email { get; }
    public Password Password { get; private set; }

    public Result ChangePassword(string currentPassword, string newPassword)
    {
        if (!this.Password.Verify(currentPassword))
            return Result.Failure(new Error("User.InvalidPassword", "Senha atual incorreta."));

        var newPasswordResult = Password.Create(newPassword);
        if (!newPasswordResult.IsSuccess)
            return Result.Failure(newPasswordResult.Error);

        this.Password = newPasswordResult.Value;
        return Result.Success();
    }
}
