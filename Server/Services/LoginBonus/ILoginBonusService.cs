namespace Server.Services.LoginBonus;

public interface ILoginBonusService
{
    IReadOnlyList<LoginBonusSummary> ListSummaries();
    long Register(LoginBonusRegistration request);
    LoginBonusRegistration? FindById(long id);
    bool DeleteById(long id);
    LoginBonusClaimResponse? ClaimForAccount(long accountId);
}
