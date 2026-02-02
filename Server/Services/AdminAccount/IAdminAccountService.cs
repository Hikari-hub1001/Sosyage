namespace Server.Services.AdminAccount;

public interface IAdminAccountService
{
    AdminAccountListResult GetAccounts(int offset, int limit);
    bool DeleteAccount(long id);
}
