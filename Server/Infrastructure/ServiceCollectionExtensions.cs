using Microsoft.Extensions.DependencyInjection;
using Server.Services.Account;
using Server.Services.AdminAccount;
using Server.Services.Assets.Item;
using Server.Services.LoginBonus;
using Server.Services.Root;

namespace Server.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAdminAccountService, AdminAccountService>();
        services.AddScoped<IAssetsItemService, AssetsItemService>();
        services.AddScoped<ILoginBonusService, LoginBonusService>();
        services.AddScoped<IRootService, RootService>();
        return services;
    }
}
