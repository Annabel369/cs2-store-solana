using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Store_Solana;

public class SolanaApi
{
    private readonly Store_SolanaConfig _config;
    private readonly HttpClient _httpClient;

    public SolanaApi(Store_SolanaConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    private async Task<string> ExecuteCommandAsync(string command)
    {
        string encodedCommand = Uri.EscapeDataString(command);
        string url = $"http://{_config.DockerApiHost}/consulta.php?apikey={_config.DockerApiKey}&comando={encodedCommand}";

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);

        if (doc.RootElement.TryGetProperty("status", out JsonElement statusElement) && statusElement.GetString() == "success")
        {
            if (doc.RootElement.TryGetProperty("output", out JsonElement outputElement))
            {
                return outputElement.GetString() ?? "";
            }
        }

        throw new Exception($"API Error: {jsonResponse}");
    }

    public async Task<double> GetSolanaBalanceAsync(string walletAddress)
    {
        string command = $"solana balance {walletAddress}";
        string output = await ExecuteCommandAsync(command);
        
        string balanceStr = output.Replace(" SOL", "").Trim();
        if (double.TryParse(balanceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double balance))
        {
            return balance;
        }

        throw new Exception($"Failed to parse balance: {output}");
    }

    public class WalletCreationResult
    {
        public string Address { get; set; } = string.Empty;
        public string PrivateKeyHex { get; set; } = string.Empty;
        public string SecretPhrase { get; set; } = string.Empty;
    }

    public async Task<WalletCreationResult> CreateWalletAsync(string playerName)
    {
        string safeName = playerName.Replace(" ", "_").ToLower();
        string walletPath = $"/solana-token/wallets/{safeName}_wallet.json";

        // Generate Wallet
        string genCommand = $"solana-keygen new --no-passphrase --outfile {walletPath} --force";
        string genOutput = await ExecuteCommandAsync(genCommand);

        string address = Regex.Match(genOutput, @"pubkey:\s+([A-Za-z0-9]+)").Groups[1].Value;
        string secretPhrase = Regex.Match(genOutput, @"Save this seed phrase to recover your new keypair:\s*\n([^\n\r=]+)", RegexOptions.Singleline).Groups[1].Value.Trim();

        if (string.IsNullOrEmpty(address))
            throw new Exception("Failed to extract wallet address from output.");

        // Read Private Key
        string readCommand = $"cat {walletPath}";
        string readOutput = await ExecuteCommandAsync(readCommand);

        // Parse JSON array to hex string
        string privateKeyHex = "";
        try
        {
            var numbers = JsonSerializer.Deserialize<int[]>(readOutput);
            if (numbers != null)
            {
                byte[] bytes = new byte[numbers.Length];
                for(int i = 0; i < numbers.Length; i++) bytes[i] = (byte)numbers[i];
                privateKeyHex = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
        catch
        {
            privateKeyHex = "INVALID_KEY";
        }

        return new WalletCreationResult
        {
            Address = address,
            PrivateKeyHex = privateKeyHex,
            SecretPhrase = secretPhrase
        };
    }

    public async Task<string> TransferSolanaAsync(string playerName, string recipientWallet, double amount)
    {
        string safeName = playerName.Replace(" ", "_").ToLower();
        string formattedAmount = amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        string command = $"solana transfer {recipientWallet} {formattedAmount} --keypair /solana-token/wallets/{safeName}_wallet.json --allow-unfunded-recipient";
        string output = await ExecuteCommandAsync(command);

        // Extract signature
        var match = Regex.Match(output, @"Signature:\s*([A-Za-z0-9]+)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        throw new Exception($"Failed to extract signature. Output: {output}");
    }
}
