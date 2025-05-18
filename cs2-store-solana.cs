using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using StoreApi;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Store_Solana;

public class Store_SolanaConfig : BasePluginConfig
{
    [JsonPropertyName("Store_Solana_commands")]
    public List<string> StoreSolanaCommands { get; set; } = ["storeSolana", "buySolana"];

    [JsonPropertyName("SolanaItems")]
    public List<SolanaItem> SolanaItems { get; set; } = new()
    {
        new SolanaItem
        {
            Name = "Pacote Pequeno de Solana",
            Options = new List<SolanaOption>
            {
                new SolanaOption { SolanaAmount = 1.0, Price = 1000 },
                new SolanaOption { SolanaAmount = 2.0, Price = 1800 }
            }
        },
        new SolanaItem
        {
            Name = "Pacote Médio de Solana",
            Options = new List<SolanaOption>
            {
                new SolanaOption { SolanaAmount = 0.5, Price = 200 },
                new SolanaOption { SolanaAmount = 1.5, Price = 600 },
                new SolanaOption { SolanaAmount = 5.0, Price = 5000 }
            }
        }
    };

    [JsonPropertyName("BalanceCommand")]
    public string BalanceCommand { get; set; } = "saldoSolana";

    [JsonPropertyName("SolanaToGameCurrencyRate")]
    public int SolanaToGameCurrencyRate { get; set; } = 1000;

    [JsonPropertyName("SolanaApiUrl")]
    public string SolanaApiUrl { get; set; } = "http://192.168.100.170/consulta.php";

    [JsonPropertyName("SolanaApiKey")]
    public string SolanaApiKey { get; set; } = "b493d48364afe44d";

    [JsonPropertyName("CreateWalletCommand")]
    public string CreateWalletCommand { get; set; } = "createwallet";

    [JsonPropertyName("WalletBankStoreAdmin")]
    public string WalletBankStoreAdmin { get; set; } = "dadhcDXHiHDrWkT2Z4pSZyF6HWmHwQMG3HtGciwccVP";
}

public class SolanaItem
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Options")]
    public List<SolanaOption> Options { get; set; } = new();
}

public class SolanaOption
{
    [JsonPropertyName("SolanaAmount")]
    public double SolanaAmount { get; set; } = 0;

    [JsonPropertyName("Price")]
    public int Price { get; set; } = 0;
}

public class Store_Solana : BasePlugin, IPluginConfig<Store_SolanaConfig>
{
    public override string ModuleName => "Store Module [Solana]";
    public override string ModuleVersion => "1.12";
    public override string ModuleAuthor => "Amauri007";

    public IStoreApi? StoreApi { get; set; }
    public Store_SolanaConfig Config { get; set; } = new();

    private readonly HttpClient _httpClient = new();

    public void OnConfigParsed(Store_SolanaConfig config)
    {
        Config = config;
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        StoreApi = IStoreApi.Capability.Get() ?? throw new Exception("StoreApi could not be located.");
        CreateCommands();
        AddCommand($"css_{Config.BalanceCommand}", "Verifica seu saldo de Solana e moedas do jogo", Command_CheckBalance);
        AddCommand($"css_{Config.CreateWalletCommand}", "Cria sua carteira Solana", Command_CreateWallet);
    }

    private void CreateCommands()
    {
        foreach (var cmd in Config.StoreSolanaCommands)
        {
            AddCommand($"css_{cmd}", "Abre o menu de compra de Solana", Command_SolanaMenu);
        }
    }

    public void Command_SolanaMenu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        var menu = new CenterHtmlMenu(Localizer["Solana Menu title"]);

        foreach (var item in Config.SolanaItems)
        {
            menu.AddMenuOption(Localizer["Solana Menu item", item.Name], (client, option) =>
            {
                OpenSolanaSubMenu(player, item);
            });
        }

        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    private void OpenSolanaSubMenu(CCSPlayerController player, SolanaItem item)
    {
        var subMenu = new CenterHtmlMenu(Localizer["Solana Options title"]);

        foreach (var option in item.Options)
        {
            subMenu.AddMenuOption(Localizer["Solana Options item", option.SolanaAmount, option.Price], (client, subOption) =>
            {
                BuySolana(player, item.Name, option);
            });
        }

        MenuManager.OpenCenterHtmlMenu(this, player, subMenu);
    }

    public async void Command_CreateWallet(CCSPlayerController? player, CommandInfo info)
    {
        // ... (seu código para Command_CreateWallet) ...
    }

    private string ExtractWalletAddress(string output)
    {
        // ... (seu código para ExtractWalletAddress) ...
    }

    public async Task<double> GetSolanaBalanceFromApi(CCSPlayerController player)
    {
        // ... (seu código para GetSolanaBalanceFromApi) ...
    }

    public async Task<bool> TransferSolana(CCSPlayerController player, double amount)
    {
        // ... (seu código para TransferSolana) ...
    }

    private string ExtractSignature(string output)
    {
        // ... (seu código para ExtractSignature) ...
    }

    private async void BuySolana(CCSPlayerController player, string SolanaName, SolanaOption option)
    {
        if (StoreApi == null) throw new Exception("StoreApi could not be located.");

        int playerCredits = StoreApi.GetPlayerCredits(player);

        if (playerCredits < option.Price)
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["Not enough credits"]);
            return;
        }

        StoreApi.GivePlayerCredits(player, -option.Price);
        double solanaAmount = option.SolanaAmount;

        if (await TransferSolana(player, solanaAmount))
        {
            int gameCurrencyAmount = (int)(solanaAmount * Config.SolanaToGameCurrencyRate);
            StoreApi.GivePlayerGameCurrency(player, gameCurrencyAmount);
            player.PrintToChat(Localizer["Prefix"] + Localizer["Solana purchased successfully", solanaAmount, SolanaName, option.Price, gameCurrencyAmount]);
        }
        else
        {
            StoreApi.GivePlayerCredits(player, option.Price);
            player.PrintToChat(Localizer["Prefix"] + Localizer["Error during Solana purchase. Credits refunded."]);
        }
    }

    public async void Command_CheckBalance(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || StoreApi == null) return;

        double solanaBalance = await GetSolanaBalanceFromApi(player);
        int gameCurrencyBalance = StoreApi.GetPlayerGameCurrencyBalance(player);

        player.PrintToChat(Localizer["Prefix"] + Localizer["Solana balance", solanaBalance]);
        player.PrintToChat(Localizer["Prefix"] + Localizer["Game currency balance", gameCurrencyBalance]);
    }
}