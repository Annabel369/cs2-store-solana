using CounterStrikeSharp.API.Core;
using StoreApi; // Certifique-se de que este namespace esteja correto

public interface IStoreApi
{
    public static ICapability<IStoreApi> Capability { get; } = new StoreApiCapability();

    int GetPlayerCredits(CCSPlayerController player);
    void GivePlayerCredits(CCSPlayerController player, int amount);
    void GivePlayerSolana(CCSPlayerController player, double amount);
    double GetPlayerSolanaBalance(CCSPlayerController player);
    void GivePlayerGameCurrency(CCSPlayerController player, int amount);
    int GetPlayerGameCurrencyBalance(CCSPlayerController player);

    void RegisterSolanaWallet(CCSPlayerController player, string walletAddress, string privateKeyInfo);
    string GetPlayerSolanaWalletAddress(CCSPlayerController player);
    void RegisterSolanaTransaction(CCSPlayerController player, string type, float value, string currency, string signature);
}

internal class StoreApiCapability : ICapability<IStoreApi>
{
    public IStoreApi Get()
    {
        return new MyStoreApiService(); // Cria e retorna uma instância da sua implementação
    }
}