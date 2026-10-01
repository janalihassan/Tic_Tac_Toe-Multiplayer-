# Tic Tac Toe Multiplayer (PlayroomKit)

An online two-player Tic Tac Toe game for the browser, made in Unity with PlayroomKit by Jan Ali Hassan. It's the classic turn-based game with the state kept in sync between both players and a simple, readable interface.

![Tic Tac Toe Multiplayer gameplay screenshot](https://janalihassan.dev/images/Tic_Tac_Toe.png)

▶️ [Watch the gameplay video](https://janalihassan.dev/videos/Tic-Tac-Toe%28PK%29.mp4) · 🌐 [More of my games at janalihassan.dev](https://janalihassan.dev/#library) · 💼 [LinkedIn post](https://lnkd.in/p/d_qHggda)

There is also a version of this game built with Unity Netcode for GameObjects: [TicTacToe-NetCode-](https://github.com/janalihassan/TicTacToe-NetCode-). This repo uses PlayroomKit for rooms and RPCs instead, and exports to WebGL.

## Features

- Two-player rooms through PlayroomKit (`InsertCoin` with `maxPlayersPerRoom = 2`)
- The host keeps the join order and sends it to the other player, so both sides agree on who is player one
- Player one plays Cross and player two plays Circle; a "YOU" label marks your symbol
- Every move goes out as a `SpawnShape` RPC and the next turn as a `SyncTurn` RPC, and an arrow shows whose turn it is
- Clicks are ignored when it isn't your turn, when the cell is taken, or after the game is over
- Win check over all rows, columns and both diagonals; the host declares the winner and sends the winning cells to everyone in a `GameOver` RPC
- A line is drawn through the three winning cells, rotated and stretched to fit
- "You Win!" / "You Lose!" text in separate win and lose colours

## Built with

- Unity 6000.0.51f1
- C#
- PlayroomKit Unity SDK (included in `Assets/PlayroomKit`)
- Universal Render Pipeline (2D renderer)
- TextMeshPro

## Key scripts

All game code is in `Assets/Script/`:

- `GameManager.cs` starts PlayroomKit, registers the RPCs (`SpawnShape`, `AssignIDs`, `UpdateLocalPlayers`, `SyncTurn`, `GameOver`), assigns symbols, validates and sends moves, checks for a winner and draws the winning line.
- `GridPosition.cs` sits on each of the nine board cells and passes its x/y to `GameManager` when clicked.
- `PlayerUI.cs` shows the "YOU" label under your symbol and moves the turn arrow.
- `GameOverUI.cs` stays hidden until the game ends, then shows "You Win!" or "You Lose!".

`Assets/PlayroomKit/` is the third-party PlayroomKit SDK with its own examples and dependencies.

## Running the project

1. Open the project in Unity Hub with Unity **6000.0.51f1**.
2. Open `Assets/Scenes/GameScene.unity` (the only scene in Build Settings).
3. PlayroomKit only supports WebGL builds, so switch the platform to WebGL and build. The project uses the `DiscordTemplate` WebGL template that comes with PlayroomKit.
4. Open the build in two browser windows on the same room link to play a match.

In the Editor, PlayroomKit falls back to a local mock with a single player, so pressing Play there won't start a match because the game waits for two players. The PlayroomKit `gameId` is set in `GameManager.Start()`.

## Author

Jan Ali Hassan · [Portfolio](https://janalihassan.dev) · [LinkedIn](https://www.linkedin.com/in/janalihassan) · [GitHub](https://github.com/janalihassan)
