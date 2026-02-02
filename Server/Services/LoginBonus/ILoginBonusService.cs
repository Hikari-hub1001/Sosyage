namespace Server.Services.LoginBonus;

public interface ILoginBonusService
{
    long Register(LoginBonusRegistration request);
    LoginBonusRegistration? FindByMonth(string month);
    bool DeleteByMonth(string month);
    LoginBonusClaimResponse? ClaimForAccount(long accountId);
}
