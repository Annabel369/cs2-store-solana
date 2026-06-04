# cs2-store-solana version test

SolanaDevMinecraft

This is a test version that I'm developing. I created one in Minecraft, and now I'm trying to get it working 

Os comandos disponíveis dentro do servidor de CS2 (pelo chat) são:

!saldo (ou /css_saldo no console): Mostra o seu saldo atual de moedas do jogo (saldo do banco) e também o seu saldo de Solana (em SOL) na rede.
!createwallet (ou /css_createwallet no console): Gera a sua carteira de Solana pelo Docker. O plugin retornará seu novo endereço (pubkey) e a frase secreta no chat do jogo e os salvará no banco de dados.
!buycurrency <quantidade_SOL> (ex: !buycurrency 1.5): Usa o SOL da sua carteira para enviar ao banco do Admin (que configuramos nas opções). Em troca, injeta moedas do jogo (saldo no banco MySQL) diretamente na sua conta. O padrão está definido para 1 SOL = 1000 moedas.
!storesolana ou !buysolana (também /css_storesolana no console): Abre o painel HTML da loja no meio da tela. Lá você pode gastar as suas "moedas do jogo" que tem no banco para comprar benefícios (como vida, coletes ou armas) configurados naqueles arquivos que deixamos prontos.
Basta os jogadores digitarem qualquer um desses comandos começando com ! no chat (como !saldo) que o servidor CS2 reconhecerá perfeitamente!

# Web Api Solana install 
https://github.com/astral123457/SolanaDevMinecraft
