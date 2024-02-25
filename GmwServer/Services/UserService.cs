
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class UserService: IUserService
{
    private readonly IDbContextFactory<GmwServerDbContext> _dbContextFactory;

    public UserService(IDbContextFactory<GmwServerDbContext> dbContextFactory){
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IServiceResult<UserId>> CreateUser(MailAddress email){
        using var db = await _dbContextFactory.CreateDbContextAsync();

        var emailAlreadyUsed = await
            (from u in db.Users
            where u.Email == email
            select true)
            .AnyAsync();

        if (emailAlreadyUsed)
            return ServiceResults.UnprocessableEntity<UserId>("Email address already registered.");

        var newUser = new User{
            Email = email,
            DisplayName = email.User
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();

        return ServiceResults.Created(newUser.Id);
    }

}

public interface IUserService
{
    Task<IServiceResult<UserId>> CreateUser(MailAddress email);
}