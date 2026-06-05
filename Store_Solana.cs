using System;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace Store_Solana;

public class Store_SolanaPlugin : BasePlugin, IPluginConfig<Store_SolanaConfig>
{
    public override string ModuleName => "Store Module [Solana Refactored]";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "Amauri007 / Refactored";

    public Store_SolanaConfig Config { get; set; } = new();
    
    private Database _database = null!;
    private SolanaApi _solanaApi = null!;

    public void OnConfigParsed(Store_SolanaConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        _database = new Database(Config);
        _solanaApi = new SolanaApi(Config);

        Task.Run(async () => await _database.InitializeAsync());
    }

    [ConsoleCommand("css_saldo", "Verifica seu saldo de Solana e moedas do jogo")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_CheckBalance(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;
        string playerName = player.PlayerName;

        Task.Run(async () =>
        {
            string? wallet = await _database.GetWalletAddressAsync(playerName);
            int gameCurrency = await _database.GetGameCurrencyBalanceAsync(playerName);

            Server.NextFrame(() =>
            {
                player.PrintToChat($"\x04[Store] \x01Saldo no Banco: \x06{gameCurrency} moedas\x01.");
            });

            if (wallet != null)
            {
                try
                {
                    double solBalance = await _solanaApi.GetSolanaBalanceAsync(wallet);
                    Server.NextFrame(() =>
                    {
                        player.PrintToChat($"\x04[Store] \x01Carteira SOL: \x0C{wallet}");
                        player.PrintToChat($"\x04[Store] \x01Saldo SOL: \x06{solBalance} SOL\x01.");
                    });
                }
                catch (Exception ex)
                {
                    Server.NextFrame(() => player.PrintToChat($"\x02[Store] Erro ao obter saldo SOL: {ex.Message}"));
                }
            }
            else
            {
                Server.NextFrame(() => player.PrintToChat("\x02[Store] Você não possui carteira SOL. Use !createwallet"));
            }
        });
    }

    [ConsoleCommand("css_createwallet", "Cria sua carteira Solana")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_CreateWallet(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;
        string playerName = player.PlayerName;

        player.PrintToChat("\x04[Store] \x01Aguarde... Conectando ao banco Solana...");

        Task.Run(async () =>
        {
            try
            {
                string? existingWallet = await _database.GetWalletAddressAsync(playerName);
                if (existingWallet != null)
                {
                    Server.NextFrame(() => player.PrintToChat($"\x02[Store] Você já possui uma carteira: \x0C{existingWallet}"));
                    return;
                }

                var wallet = await _solanaApi.CreateWalletAsync(playerName);
                bool registered = await _database.RegisterWalletAsync(playerName, wallet.Address, wallet.PrivateKeyHex, wallet.SecretPhrase);

                Server.NextFrame(() =>
                {
                    if (registered)
                    {
                        player.PrintToChat($"\x04[Store] \x01Carteira criada com sucesso!");
                        player.PrintToChat($"\x04[Store] \x01Endereço: \x0C{wallet.Address}");
                        player.PrintToChat($"\x04[Store] \x01Frase secreta: \x0A{wallet.SecretPhrase}");
                        player.PrintToChat("\x02[Aviso] \x01Guarde sua frase secreta com segurança!");
                    }
                    else
                    {
                        player.PrintToChat("\x02[Store] Erro ao salvar sua carteira no banco de dados.");
                    }
                });
            }
            catch (Exception ex)
            {
                Server.NextFrame(() => player.PrintToChat($"\x02[Store] Erro ao criar carteira: {ex.Message}"));
            }
        });
    }

    [ConsoleCommand("css_buycurrency", "Compra moedas do jogo usando SOL")]
    [CommandHelper(minArgs: 1, usage: "<quantidade_SOL>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_BuyCurrency(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;
        string playerName = player.PlayerName;

        if (!double.TryParse(info.GetArg(1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double solAmount))
        {
            player.PrintToChat("\x02[Store] Uso: !buycurrency <quantidade_SOL>");
            return;
        }

        player.PrintToChat("\x04[Store] \x01Processando transferência...");

        Task.Run(async () =>
        {
            try
            {
                string? wallet = await _database.GetWalletAddressAsync(playerName);
                if (wallet == null)
                {
                    Server.NextFrame(() => player.PrintToChat("\x02[Store] Você não possui carteira SOL. Use !createwallet"));
                    return;
                }

                double currentSol = await _solanaApi.GetSolanaBalanceAsync(wallet);
                if (currentSol < solAmount)
                {
                    Server.NextFrame(() => player.PrintToChat($"\x02[Store] Saldo insuficiente. Você tem \x06{currentSol} SOL\x02."));
                    return;
                }

                string signature = await _solanaApi.TransferSolanaAsync(playerName, Config.WalletBankStoreAdmin, solAmount);
                int gameCurrencyAmount = (int)(solAmount * Config.SolanaToGameCurrencyRate);

                await _database.UpdateGameCurrencyBalanceAsync(playerName, gameCurrencyAmount);
                await _database.LogTransactionAsync(playerName, "compra_moedas", solAmount, "SOL", signature);

                Server.NextFrame(() =>
                {
                    player.PrintToChat($"\x04[Store] \x01Compra realizada com sucesso!");
                    player.PrintToChat($"\x04[Store] \x01Você recebeu \x06{gameCurrencyAmount} moedas\x01.");
                    player.PrintToChat($"\x04[Store] \x01Assinatura: \x0A{signature}");
                });
            }
            catch (Exception ex)
            {
                Server.NextFrame(() => player.PrintToChat($"\x02[Store] Erro ao processar compra: {ex.Message}"));
            }
        });
    }

    [ConsoleCommand("css_storesolana", "Abre a loja do servidor")]
    [ConsoleCommand("css_buysolana", "Abre a loja do servidor")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_StoreSolana(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        var menu = new CenterHtmlMenu("Loja do Servidor", this);

        foreach (var item in Config.StoreItems)
        {
            menu.AddMenuOption($"{item.Name} - {item.Price} moedas", (client, option) =>
            {
                ProcessItemPurchase(client, item);
            });
        }

        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    private void ProcessItemPurchase(CCSPlayerController player, StoreItem item)
    {
        string playerName = player.PlayerName;
        Task.Run(async () =>
        {
            try
            {
                int balance = await _database.GetGameCurrencyBalanceAsync(playerName);

                if (balance < item.Price)
                {
                    Server.NextFrame(() => player.PrintToChat($"\x02[Store] Saldo insuficiente. Você tem {balance} moedas. Custa {item.Price}."));
                    return;
                }

                bool success = await _database.UpdateGameCurrencyBalanceAsync(playerName, -item.Price);
                
                if (success)
                {
                    Server.NextFrame(() =>
                    {
                        ExecuteItemCommand(player, item.CommandToExecute);
                        player.PrintToChat($"\x04[Store] \x01Você comprou \x0C{item.Name}\x01 por \x06{item.Price} moedas\x01.");
                    });
                }
            }
            catch (Exception ex)
            {
                Server.NextFrame(() => player.PrintToChat($"\x02[Store] Erro ao comprar item: {ex.Message}"));
            }
        });
    }

    private void ExecuteItemCommand(CCSPlayerController player, string command)
    {
        if (!player.IsValid || !player.PawnIsAlive) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        switch (command)
        {
            case "give_health":
                pawn.Health += 50;
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                break;
            case "give_armor":
                pawn.ArmorValue = 100;
                Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
                break;
            case "give_awp":
                player.GiveNamedItem("weapon_awp");
                break;
            default:
                player.PrintToChat("\x02[Store] Benefício não reconhecido pelo servidor.");
                break;
        }
    }
}
