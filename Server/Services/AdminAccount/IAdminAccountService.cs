namespace Server.Services.AdminAccount;

public interface IAdminAccountService
{
    AdminAccountListResult GetAccounts(int offset, int limit);
    AdminAccountDetailResult? GetAccountDetail(long id);
    bool DeleteAccount(long id);
}
