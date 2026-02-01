namespace Server.Services.Account;

public interface IAccountService
{
    long Register(string name);
    string? Login(long id);
}
