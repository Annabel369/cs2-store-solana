using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CounterStrikeSharp.API.Core;
using System.Data.MySqlClient; // Se você estiver usando MySQL

namespace StoreApi; // Certifique-se de usar o mesmo namespace da sua interface

public class MyStoreApiService : IStoreApi
{
    private readonly string _connectionString = "SEU_STRING_DE_CONEXAO_MYSQL"; // Substitua pela sua string de conexão

    public MyStoreApiService()
    {
        // Você pode inicializar conexões ou outros recursos aqui, se necessário
    }

    public int GetPlayerCredits(CCSPlayerController player)
    {
        // Implemente a lógica para obter os créditos do jogador do banco de dados
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT saldo FROM banco WHERE jogador = @jogador";
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0; // Ou algum valor padrão
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao obter créditos de {player.PlayerName}: {ex.Message}");
            return 0;
        }
    }

    public void GivePlayerCredits(CCSPlayerController player, int amount)
    {
        // Implemente a lógica para dar ou remover créditos do jogador no banco de dados
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE banco SET saldo = saldo + @amount WHERE jogador = @jogador";
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao dar/remover créditos de {player.PlayerName}: {ex.Message}");
        }
    }

    public void GivePlayerSolana(CCSPlayerController player, double amount)
    {
        // Implemente a lógica para dar "Solana" (virtual) ao jogador (pode ser no banco de dados)
        // Você precisará de uma tabela para armazenar o saldo de "Solana" dos jogadores
        // Exemplo (adapte para sua estrutura):
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            // Verifique se o jogador já tem um registro de saldo de Solana e atualize ou insira
            string checkIfExistsQuery = "SELECT COUNT(*) FROM carteiras_solana WHERE jogador_id = (SELECT id FROM jogadores WHERE nome = @jogador)";
            using var checkCommand = new MySqlCommand(checkIfExistsQuery, connection);
            checkCommand.Parameters.AddWithValue("@jogador", player.PlayerName);
            long count = (long)checkCommand.ExecuteScalar();

            string query;
            if (count > 0)
            {
                query = "UPDATE carteiras_solana SET saldo_solana = saldo_solana + @amount WHERE jogador_id = (SELECT id FROM jogadores WHERE nome = @jogador)";
            }
            else
            {
                query = "INSERT INTO carteiras_solana (jogador_id, saldo_solana) VALUES ((SELECT id FROM jogadores WHERE nome = @jogador), @amount)";
            }

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao dar Solana para {player.PlayerName}: {ex.Message}");
        }
    }

    public double GetPlayerSolanaBalance(CCSPlayerController player)
    {
        // Implemente a lógica para obter o saldo de "Solana" do jogador do banco de dados
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT saldo_solana FROM carteiras_solana WHERE jogador_id = (SELECT id FROM jogadores WHERE nome = @jogador)";
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToDouble(result) : 0.0;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao obter saldo de Solana de {player.PlayerName}: {ex.Message}");
            return 0.0;
        }
    }

    public void GivePlayerGameCurrency(CCSPlayerController player, int amount)
    {
        // Implemente a lógica para dar "moedas do jogo" (se diferente dos créditos)
        // Adapte para sua tabela e coluna
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE banco SET saldo = saldo + @amount WHERE jogador = @jogador"; // Assumindo que usa a mesma tabela
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao dar moedas do jogo para {player.PlayerName}: {ex.Message}");
        }
    }

    public int GetPlayerGameCurrencyBalance(CCSPlayerController player)
    {
        // Implemente a lógica para obter o saldo de "moedas do jogo"
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT saldo FROM banco WHERE jogador = @jogador"; // Assumindo que usa a mesma tabela
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao obter saldo de moedas do jogo de {player.PlayerName}: {ex.Message}");
            return 0;
        }
    }

    public void RegisterSolanaWallet(CCSPlayerController player, string walletAddress, string privateKeyInfo)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Primeiro, obtenha o ID do jogador
            using var getPlayerIdCommand = new MySqlCommand("SELECT id FROM jogadores WHERE nome = @nome", connection);
            getPlayerIdCommand.Parameters.AddWithValue("@nome", player.PlayerName);
            var playerIdResult = getPlayerIdCommand.ExecuteScalar();
            if (playerIdResult == null)
            {
                Console.WriteLine($"[Store_Solana] Jogador {player.PlayerName} não encontrado na tabela 'jogadores'.");
                return;
            }
            int playerId = Convert.ToInt32(playerIdResult);

            // Insira a carteira na tabela 'carteiras'
            using var insertWalletCommand = new MySqlCommand("INSERT INTO carteiras (jogador_id, endereco, chave_privada, frase_secreta) VALUES (@jogadorId, @endereco, @chavePrivada, '')", connection);
            insertWalletCommand.Parameters.AddWithValue("@jogadorId", playerId);
            insertWalletCommand.Parameters.AddWithValue("@endereco", walletAddress);
            insertWalletCommand.Parameters.AddWithValue("@chavePrivada", privateKeyInfo); // Você pode querer armazenar isso de forma mais segura
            insertWalletCommand.ExecuteNonQuery();

            Console.WriteLine($"[Store_Solana] Carteira registrada para {player.PlayerName}: {walletAddress}");
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao registrar carteira de {player.PlayerName}: {ex.Message}");
        }
    }

    public string GetPlayerSolanaWalletAddress(CCSPlayerController player)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT c.endereco FROM carteiras c JOIN jogadores j ON c.jogador_id = j.id WHERE j.nome = @jogador", connection);
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            var result = command.ExecuteScalar();
            return result?.ToString();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao obter endereço da carteira de {player.PlayerName}: {ex.Message}");
            return null;
        }
    }

    public void RegisterSolanaTransaction(CCSPlayerController player, string type, float value, string currency, string signature)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = new MySqlCommand("INSERT INTO livro_caixa (jogador, tipo_transacao, valor, moeda, assinatura) VALUES (@jogador, @tipo, @valor, @moeda, @assinatura)", connection);
            command.Parameters.AddWithValue("@jogador", player.PlayerName);
            command.Parameters.AddWithValue("@tipo", type);
            command.Parameters.AddWithValue("@valor", value);
            command.Parameters.AddWithValue("@moeda", currency);
            command.Parameters.AddWithValue("@assinatura", signature);
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"[Store_Solana] Erro ao registrar transação de {player.PlayerName}: {ex.Message}");
        }
    }
}
