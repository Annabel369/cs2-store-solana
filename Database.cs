using System;
using System.Threading.Tasks;
using MySqlConnector;

namespace Store_Solana;

public class Database
{
    private readonly string _connectionString;

    public Database(Store_SolanaConfig config)
    {
        _connectionString = $"Server={config.DatabaseHost};Port={config.DatabasePort};User ID={config.DatabaseUser};Password={config.DatabasePassword};Database={config.DatabaseName};";
    }

    public async Task InitializeAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string createTablesQuery = @"
                CREATE TABLE IF NOT EXISTS jogadores (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    nome VARCHAR(255) UNIQUE
                );
                CREATE TABLE IF NOT EXISTS carteiras (
                    jogador_id INT,
                    endereco VARCHAR(255),
                    chave_privada TEXT,
                    frase_secreta TEXT,
                    FOREIGN KEY(jogador_id) REFERENCES jogadores(id)
                );
                CREATE TABLE IF NOT EXISTS banco (
                    jogador VARCHAR(255) PRIMARY KEY,
                    saldo INT DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS livro_caixa (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    jogador VARCHAR(255),
                    tipo_transacao VARCHAR(50),
                    valor DOUBLE,
                    moeda VARCHAR(10),
                    assinatura VARCHAR(255),
                    data_hora TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

            using var command = new MySqlCommand(createTablesQuery, connection);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao inicializar banco de dados: {ex.Message}");
        }
    }

    public async Task<int> GetGameCurrencyBalanceAsync(string playerName)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SELECT saldo FROM banco WHERE jogador = @jogador", connection);
            command.Parameters.AddWithValue("@jogador", playerName);
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao obter saldo de moedas do jogo: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> UpdateGameCurrencyBalanceAsync(string playerName, int amountToAdd)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Check if player exists in banco
            using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM banco WHERE jogador = @jogador", connection);
            checkCmd.Parameters.AddWithValue("@jogador", playerName);
            long count = (long)await checkCmd.ExecuteScalarAsync();

            string query;
            if (count > 0)
            {
                query = "UPDATE banco SET saldo = saldo + @amount WHERE jogador = @jogador";
            }
            else
            {
                query = "INSERT INTO banco (jogador, saldo) VALUES (@jogador, @amount)";
            }

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@amount", amountToAdd);
            cmd.Parameters.AddWithValue("@jogador", playerName);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao atualizar saldo de moedas: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> GetWalletAddressAsync(string playerName)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SELECT c.endereco FROM carteiras c JOIN jogadores j ON c.jogador_id = j.id WHERE j.nome = @jogador", connection);
            command.Parameters.AddWithValue("@jogador", playerName);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao obter carteira: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> RegisterWalletAsync(string playerName, string walletAddress, string privateKey, string secretPhrase)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var checkPlayer = new MySqlCommand("SELECT id FROM jogadores WHERE nome = @nome", connection);
            checkPlayer.Parameters.AddWithValue("@nome", playerName);
            var playerIdResult = await checkPlayer.ExecuteScalarAsync();
            int playerId;

            if (playerIdResult == null)
            {
                using var createPlayer = new MySqlCommand("INSERT INTO jogadores (nome) VALUES (@nome)", connection);
                createPlayer.Parameters.AddWithValue("@nome", playerName);
                await createPlayer.ExecuteNonQueryAsync();
                playerId = (int)createPlayer.LastInsertedId;
            }
            else
            {
                playerId = Convert.ToInt32(playerIdResult);
            }

            using var statement = new MySqlCommand(
                "INSERT INTO carteiras (jogador_id, endereco, chave_privada, frase_secreta) VALUES (@id, @addr, @priv, @sec) " +
                "ON DUPLICATE KEY UPDATE endereco = @addr, chave_privada = @priv, frase_secreta = @sec", connection);
            statement.Parameters.AddWithValue("@id", playerId);
            statement.Parameters.AddWithValue("@addr", walletAddress);
            statement.Parameters.AddWithValue("@priv", privateKey);
            statement.Parameters.AddWithValue("@sec", secretPhrase);
            await statement.ExecuteNonQueryAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao registrar carteira: {ex.Message}");
            return false;
        }
    }

    public async Task LogTransactionAsync(string player, string type, double value, string currency, string signature)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("INSERT INTO livro_caixa (jogador, tipo_transacao, valor, moeda, assinatura) VALUES (@jogador, @tipo, @valor, @moeda, @assinatura)", connection);
            command.Parameters.AddWithValue("@jogador", player);
            command.Parameters.AddWithValue("@tipo", type);
            command.Parameters.AddWithValue("@valor", value);
            command.Parameters.AddWithValue("@moeda", currency);
            command.Parameters.AddWithValue("@assinatura", signature);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao registrar transação: {ex.Message}");
        }
    }
}
