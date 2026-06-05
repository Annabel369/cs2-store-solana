
# 🟣 CS2 Store Solana

Plugin para **Counter-Strike 2** com **CounterStrikeSharp** que integra carteiras **Solana** ao servidor, permitindo que jogadores criem carteiras, consultem saldo em SOL e comprem itens/moedas do jogo usando criptomoeda real.

Baseado e portado do projeto [SolanaDevMinecraft](https://github.com/Annabel369/SolanaDevMinecraft) para o CS2.

---

## 📋 Requisitos

| Requisito | Versão |
|-----------|--------|
| CounterStrikeSharp | ≥ 1.0.266 |
| .NET SDK | 8.0 |
| MySQL / MariaDB | 5.7+ |
| Docker | qualquer versão recente |
| Imagem Docker Solana | `heysolana` |
| PHP (para painel web) | 7.4+ |

---

## ⚙️ Instalação

### 1. Compilar o plugin

```bash
docker run --rm \
  -v $(pwd)/Store_Solana:/app \
  -w /app \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet publish -c Release
```

### 2. Copiar para o servidor

```bash
cp -r Store_Solana/bin/Release/net8.0/publish/* \
  /caminho/cs2/game/csgo/addons/counterstrikesharp/plugins/Store_Solana/

# ⚠️ IMPORTANTE: remova o CounterStrikeSharp.API.dll da pasta do plugin
rm /caminho/cs2/game/csgo/addons/counterstrikesharp/plugins/Store_Solana/CounterStrikeSharp.API.dll
```

### 3. Configurar o plugin

Edite o arquivo `Store_Solana.json` gerado na pasta do plugin:

```json
{
  "DatabaseHost": "localhost",
  "DatabasePort": 3306,
  "DatabaseUser": "root",
  "DatabasePassword": "sua_senha",
  "DatabaseName": "solanadev",
  "SolanaToGameCurrencyRate": 1000,
  "DockerApiHost": "localhost",
  "DockerApiKey": "sua_api_key",
  "WalletBankStoreAdmin": "endereco_carteira_admin",
  "StoreItems": [
    { "Name": "Vida Extra (+50 HP)", "Price": 500, "CommandToExecute": "give_health" },
    { "Name": "Colete + Capacete",   "Price": 1000, "CommandToExecute": "give_armor" },
    { "Name": "AWP",                  "Price": 4750, "CommandToExecute": "give_awp" }
  ]
}
```

### 4. Configurar o banco de dados

O plugin cria as tabelas automaticamente ao iniciar. Estrutura gerada:

```sql
-- Jogadores registrados
CREATE TABLE jogadores (
    id   INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(255) UNIQUE
);

-- Carteiras Solana de cada jogador
CREATE TABLE carteiras (
    jogador_id   INT,
    endereco     VARCHAR(255),
    chave_privada TEXT,
    frase_secreta TEXT,
    FOREIGN KEY(jogador_id) REFERENCES jogadores(id)
);

-- Saldo de moedas do jogo
CREATE TABLE banco (
    jogador VARCHAR(255) PRIMARY KEY,
    saldo   INT DEFAULT 0
);

-- Histórico de transações
CREATE TABLE livro_caixa (
    id             INT AUTO_INCREMENT PRIMARY KEY,
    jogador        VARCHAR(255),
    tipo_transacao VARCHAR(50),
    valor          DOUBLE,
    moeda          VARCHAR(10),
    assinatura     VARCHAR(255),
    data_hora      TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## 🎮 Comandos no jogo

| Comando | Alias | Descrição |
|---------|-------|-----------|
| `!createwallet` | `!criarcarteira` | Cria uma carteira Solana para o jogador |
| `!saldo` | `!balance` | Mostra saldo em moedas e SOL |
| `!buycurrency <SOL>` | — | Compra moedas do jogo com SOL (ex: `!buycurrency 0.5`) |
| `!storesolana` | `!buysolana` | Abre o menu da loja do servidor |

### Console admin

```
css_plugins list              → lista plugins carregados
css_plugins reload Store_Solana  → recarrega o plugin sem reiniciar o servidor
```

---

## 🛒 Itens da loja (padrão)

| Item | Preço (moedas) | Efeito |
|------|---------------|--------|
| Vida Extra (+50 HP) | 500 | +50 HP imediato |
| Colete + Capacete | 1000 | Armor = 100 |
| AWP | 4750 | Recebe uma AWP |

> Adicione mais itens no `Store_Solana.json` com o campo `CommandToExecute`.

---

## 🌐 Painel Web (PHP)

O painel web em PHP (`carteira.php`) permite visualizar:
- Carteiras dos jogadores
- Saldo SOL em tempo real (via Docker)
- Chave privada (oculta, mostrar ao passar o mouse)
- Frase secreta (oculta, mostrar ao passar o mouse)

> Conecta ao banco `solanadev` — o mesmo banco usado pelo CS2 e pelo Minecraft.

---

## 🔌 Plugins instalados no servidor

| Plugin | Versão | Autor | Descrição |
|--------|--------|-------|-----------|
| **Store_Solana** | 2.0.0 | Amauri007 / Refactored | Carteira Solana + Loja integrada |
| **Admin Control with MySQL & CFG Sync** | 16.0.2 | Amauri Bueno dos Santos & Gemini | Banimentos, admins e RCON com MySQL |
| **Frozen_Elsa** | 5.0.5 | Astral + Copilot | Efeitos especiais de granadas (estilo Elsa) |
| **AdminSystem (Unificado)** | 1.0.2 | Antigravity | Sistema de administração unificado |

---

## 🔗 Como funciona a integração Solana

```
Jogador usa !createwallet
        ↓
CS2 Plugin (C#) → HTTP GET → consulta.php (PHP)
        ↓
consulta.php → Docker → heysolana (solana-keygen new)
        ↓
Retorna: endereço + frase secreta + chave privada JSON
        ↓
CS2 Plugin salva tudo no MySQL (banco: solanadev)
        ↓
Jogador vê carteira na web em carteira.php
```

---

## 📁 Estrutura do projeto

```
Store_Solana/
├── Store_Solana.cs        # Plugin principal (comandos, menus)
├── Store_SolanaConfig.cs  # Configurações (lidas do JSON)
├── Database.cs            # Acesso ao MySQL
├── SolanaApi.cs           # Comunicação com Docker/consulta.php
└── Store_Solana.csproj    # Projeto .NET 8.0
```

---

## 🤝 Baseado em

- [SolanaDevMinecraft (Folia)](https://github.com/Annabel369/SolanaDevMinecraft) — lógica original em Java/Bukkit
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) — framework para plugins CS2

---

## ⚠️ Aviso

Este projeto utiliza a **rede de desenvolvimento (devnet) da Solana**. Não use com SOL real sem as devidas precauções de segurança.

# Web Api Solana install 
https://github.com/astral123457/SolanaDevMinecraft
