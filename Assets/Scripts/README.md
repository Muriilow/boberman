# Documentação Técnica - Scripts (Boberman)

Este documento descreve a organização, arquitetura e responsabilidade dos scripts no projeto **Boberman**, um clone de Bomberman focado em multiplayer via Steam.

## Arquitetura Geral

O projeto utiliza **Unity Netcode for GameObjects** para a camada de rede e **Facepunch.Steamworks** para integração com a Steam (Lobby, P2P Transport).

A lógica de personagens (Jogadores e Inimigos) é baseada no padrão **State Machine (Máquina de Estados)** para garantir comportamentos modulares e fáceis de depurar.

---

## Organização de Diretórios

### 1. `Animations/`
Contém `StateMachineBehaviour` que estendem a lógica do Animator.
- **ExplosionDies.cs / WallDies.cs:** Garantem que os objetos sejam destruídos/despawnados no servidor assim que a animação de morte ou explosão terminar.

### 2. `Enemies/`
Sistema de IA modular para inimigos.
- **`Base/Enemy.cs`:** Classe principal que gerencia a vida, movimento e a máquina de estados do inimigo.
- **`BehaviourLogic/`:** Utiliza **ScriptableObjects** para definir comportamentos de *Idle*, *Chase* e *Attack*. Isso permite criar diferentes tipos de inimigos apenas trocando os assets de lógica.
- **`StateMachine/`:** Implementações concretas dos estados de IA.
- **`TriggerCheck/`:** Sensores 2D para detectar o jogador (Aggro e Distância de Ataque).

### 3. `GameManager/`
Scripts de controle global do jogo.
- **Grid.cs:** Estruturas de dados para o sistema de grid, incluindo serialização de rede para bombas e tiles.
- **ManageDrops.cs:** Controla a geração procedural do mapa (paredes), detecção de tiles usáveis e o spawn de Power-Ups.
- **ManageRounds.cs:** O "Cérebro" da partida. Gerencia o ciclo de vida do jogo (início, fim de round, condições de vitória, respawn de jogadores) e integração com o Steam Lobby.

### 4. `Lobby/`
- **Lobby.cs (SteamLobby):** Singleton responsável por criar, entrar, listar membros e gerenciar a comunicação com a API de Matchmaking da Steam.

### 5. `Player/`
Lógica central do jogador.
- **PlayerManager.cs:** Ponto central do jogador, gerencia vida e a máquina de estados.
- **PlayerMovement.cs:** Lógica de movimentação baseada em física.
- **PlayerBomb.cs:** Gerencia a contagem de bombas, raio de explosão e a colocação de bombas no grid sincronizado.
- **StateMachine/:** Estados específicos como *Walking*, *Idle* e *Die*.

### 6. `PowerUps/`
- **PowerUpBase.cs:** ScriptableObject base para definições de itens.
- **`Pick/`:** Scripts para os objetos coletáveis no cenário. Incluem um `CountdownTimer` para que o item desapareça após um tempo se não for coletado.

### 7. `StateMachineBase/`
- **IState.cs / StateMachine.cs:** Implementação genérica do padrão State Machine utilizada por todo o projeto para desacoplar lógica de estado de lógica de MonoBehaviours.

### 8. `Steamworks.NET/`
- **FacepunchTransport.cs:** Implementação customizada do transporte do Unity Netcode para funcionar sobre a rede P2P da Steam.
- **SteamManager.cs:** Inicializa e mantém a conexão com o cliente Steam.

### 9. `UI/`
- Componentes de interface para lista de amigos, convites de Steam e botões de controle de Lobby.

### 10. `Utilities/`
- **Timer.cs:** Implementações de timers (Countdown/Stopwatch) usados para mecânicas de jogo e rede.
- **ConvertPosition.cs:** Utilitário para converter coordenadas de mundo para índices do grid (X, Y).

---

## 🌐 Sincronização de Rede (Netcode)
- **RPCs (Remote Procedure Calls):** Usados para ações pontuais como `SpawnBombServerRpc` ou `SetDirectionClientRpc`.
- **NetworkVariables:** Usadas para sincronizar estados contínuos como a quantidade de bombas restantes ou raio da explosão.
- **ClientNetworkTransform:** Utilizado para permitir que o dono do objeto (o jogador) tenha autoridade sobre sua posição, reduzindo a sensação de lag.
