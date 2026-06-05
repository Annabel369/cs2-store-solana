using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Store_Solana;

public class StoreItem
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Price")]
    public int Price { get; set; } = 0;

    [JsonPropertyName("CommandToExecute")]
    public string CommandToExecute { get; set; } = string.Empty;
}

public class Store_SolanaConfig : BasePluginConfig
{
    [JsonPropertyName("DatabaseHost")]
    public string DatabaseHost { get; set; } = "localhost";

    [JsonPropertyName("DatabasePort")]
    public int DatabasePort { get; set; } = 3306;

    [JsonPropertyName("DatabaseUser")]
    public string DatabaseUser { get; set; } = "root";

    [JsonPropertyName("DatabasePassword")]
    public string DatabasePassword { get; set; } = "0073007"; // Senha do banco usada no Minecraft

    [JsonPropertyName("DatabaseName")]
    public string DatabaseName { get; set; } = "solanadev";

    [JsonPropertyName("SolanaToGameCurrencyRate")]
    public int SolanaToGameCurrencyRate { get; set; } = 1000;

    [JsonPropertyName("DockerApiHost")]
    public string DockerApiHost { get; set; } = "localhost";

    [JsonPropertyName("DockerApiKey")]
    public string DockerApiKey { get; set; } = "b493d48364afe44d";

    [JsonPropertyName("WalletBankStoreAdmin")]
    public string WalletBankStoreAdmin { get; set; } = "dadhcDXHiHDrWkT2Z4pSZyF6HWmHwQMG3HtGciwccVP";

    [JsonPropertyName("StoreItems")]
    public List<StoreItem> StoreItems { get; set; } = new List<StoreItem>
    {
        new StoreItem { Name = "Vida Extra (+50 HP)", Price = 500, CommandToExecute = "give_health" },
        new StoreItem { Name = "Colete + Capacete", Price = 1000, CommandToExecute = "give_armor" },
        new StoreItem { Name = "AWP", Price = 4750, CommandToExecute = "give_awp" }
    };
}
