using System;
using UnityEngine;
using Playroom;
using static Playroom.PlayroomKit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private const float GRID_SIZE = 3.1f;
    public static GameManager Instance { get; private set; }

    public event EventHandler OnShapeAssign;
    public event Action<string> OnGameOver;
    public event Action<string> OnTurnchanged;

    private static bool playerJoined;
    private PlayroomKit _playroomKit = new();

    [SerializeField] private GameObject cross;
    [SerializeField] private GameObject cricle;
    [SerializeField] private GameObject winningLine;

    private string currentTurnId = "";
    private PlayroomKit.Player playerOne;
    private PlayroomKit.Player playerTwo;
    private string shapeType;
    private bool gameOver;

    private readonly Dictionary<Vector2Int, string> boardState = new();
    private readonly HashSet<Vector2Int> occupiedCells = new();
    private static readonly List<string> playersIds = new();
    private static Dictionary<string, GameObject> PlayerDict = new();
    private static readonly List<PlayroomKit.Player> players = new();
    private static readonly List<GameObject> playerGameObjects = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _playroomKit.InsertCoin(new InitOptions()
        {
            gameId = "8S1HdfCLr5WhisdRN1gc",
            maxPlayersPerRoom = 2,
            defaultPlayerStates = new()
            {
            {"score", 0},

            },
        }, () =>
        {
            _playroomKit.OnPlayerJoin(AddPlayer);
            _playroomKit.RpcRegister("SpawnShape", RpcSpawnShape);
            _playroomKit.RpcRegister("AssignIDs", RpcAssignIds);
            _playroomKit.RpcRegister("UpdateLocalPlayers", RpcUpdatePlayers);
            _playroomKit.RpcRegister("SyncTurn", SyncTurn);
            _playroomKit.RpcRegister("GameOver", GameOver);

        });
    }

    private void GameOver(string data, string senderId)
    {
        gameOver = true;

        var parts = data.Split('|');
        string winnerId = parts[0];

        string[] cellStrings = parts[1].Split(";");

        Vector2Int[] winningCells = cellStrings.Select(s =>
        {
            var xy = s.Split(',');
            return new Vector2Int(int.Parse(xy[0]), int.Parse(xy[1]));
        }).ToArray();

        DrawWinningLine(winningCells);

        OnGameOver?.Invoke(winnerId);
        Debug.Log($"Game Over! Winner is {winnerId}");
    }

    private void DrawWinningLine(Vector2Int[] winningCells)
    {
        if(winningCells.Length != 3)return;

        Vector3 start = GetGridWorldPosition(winningCells[0].x, winningCells[0].y);
        Vector3 end = GetGridWorldPosition(winningCells[2].x, winningCells[2].y);

        Vector2 center = (start + end) / 2f;
        Vector2 direction = end - start;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float length = direction.magnitude;

        GameObject line = Instantiate(winningLine, center, Quaternion.Euler(0, 0, angle));
        line.transform.localScale = new Vector3(length, line.transform.localScale.y, 1f);

    }

    private void SyncTurn(string playerId, string senderId)
    {
        currentTurnId = playerId;
        Debug.Log("Turn synced: now it's " + playerId + "'s turn");

        OnTurnchanged?.Invoke(currentTurnId);
    }

    private void RpcUpdatePlayers(string data, string arg2)
    {
        List<string> newOrderIds = data.Split(',').Select(id => id.Trim()).Where(id => !string.IsNullOrEmpty(id)).ToList();

        playersIds.Clear();
        playersIds.AddRange(newOrderIds);

        if (playersIds.Count == 2)
        {
            _playroomKit.RpcCall("AssignIDs", "");
        }
    }

    private void RpcAssignIds(string arg1, string arg2)
    {
        var reorderedPlayers = playersIds.Select(id => players.FirstOrDefault(p => p.id == id)).Where(p => p != null).ToList();
        players.Clear();
        players.AddRange(reorderedPlayers);

        if (players.Count < 2)
        {
            return;
        }

        playerOne = players[0];
        playerTwo = players[1];

        if (_playroomKit.MyPlayer().id == playerOne.id)
        {
            shapeType = "CROSS";
        }
        else if (_playroomKit.MyPlayer().id == playerTwo.id)
        {
            shapeType = "CIRCLE";
        }
        else
        {
            Debug.LogWarning("Invalid shape type");
            return;
        }

        if (_playroomKit.IsHost() && players.Count == 2)
        {
            currentTurnId = playerOne.id;
            _playroomKit.RpcCall("SyncTurn", currentTurnId, PlayroomKit.RpcMode.ALL);
        }

        OnShapeAssign?.Invoke(this, EventArgs.Empty);
    }

    private void RpcSpawnShape(string data, string senderId)
    {
        var parts = data.Split(',');
        string shapeType = parts[0];
        int x = int.Parse(parts[1]);
        int y = int.Parse(parts[2]);



        Vector2 spawnPos = GetGridWorldPosition(x, y);

        if (shapeType == "CROSS")
        {
            Instantiate(cross, spawnPos, Quaternion.identity);
        }
        else if (shapeType == "CIRCLE")
        {
            Instantiate(cricle, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"Unknown shape type: {shapeType}");
        }

        Vector2Int gridPos = new(x, y);


        occupiedCells.Add(gridPos);
        boardState[gridPos] = shapeType;

        CheckWinner();
    }

    private void Update()
    {
        if (PlayerDict.Count > 0)
        {
            LocalPlayerSet();
            GetOtherPlayers();
        }
    }

    private void CheckWinner()
    {
        string winner;
        Vector2Int[] winningCells = null;   

        for (int i = 0; i < 3; i++)
        {
            //Horizontal Line
            if (HasLine(new Vector2Int(i, 0), new Vector2Int(i, 1), new Vector2Int(i, 2), out winner))
            {
                winningCells = new[] { new Vector2Int(i, 0), new Vector2Int(i, 1), new Vector2Int(i, 2) };
                DeclareWinner(winner,winningCells);
                return;
            }
            //vertical line
            if (HasLine(new Vector2Int(0, i), new Vector2Int(1, i), new Vector2Int(2, i), out winner))
            {
                winningCells = new[] { new Vector2Int(0, i), new Vector2Int(1, i), new Vector2Int(2, i), };
                DeclareWinner(winner,winningCells);
                return;

            }
        }
        // Diagonals
        if (HasLine(new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2), out winner))
        {
            winningCells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2), };
            DeclareWinner(winner,winningCells);
            return;
        }
        if (HasLine(new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0), out winner))
        {
            winningCells = new[] {new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0) };
            DeclareWinner(winner,winningCells);
            return;
        }
    }

    private bool HasLine(Vector2Int a, Vector2Int b, Vector2Int c, out string winner)
    {
        winner = null;
        if (!boardState.ContainsKey(a) || !boardState.ContainsKey(b) || !boardState.ContainsKey(c))
        {
            return false;
        }

        string ShapeA = boardState[a];
        string ShapeB = boardState[b];
        string ShapeC = boardState[c];

        if (ShapeA == ShapeB && ShapeB == ShapeC)
        {
            winner = ShapeA;
            return true;
        }
        return false;

    }

    private void DeclareWinner(string winnerShape, Vector2Int[] winningCells)
    {
        if (!_playroomKit.IsHost()) return;

        string winnerId = (winnerShape == "CROSS")? playerOne.id : playerTwo.id;

        DrawWinningLine(winningCells);

        string cellsData = string.Join(";",winningCells.Select(c=> $"{c.x},{c.y}"));
        string data = $"{winnerId}|{cellsData}";

        _playroomKit.RpcCall("GameOver", data, RpcMode.ALL);
    }


    private void LocalPlayerSet()
    {
        if (playerJoined)
        {
            if (PlayerDict.TryGetValue(_playroomKit.MyPlayer().id, out GameObject p))
            {
                _playroomKit.MyPlayer().SetState("pos", p.transform.position);
            }
        }
    }

    private void GetOtherPlayers()
    {
        foreach (KeyValuePair<string, GameObject> player in PlayerDict)
        {
            if (player.Key == _playroomKit.MyPlayer().id) continue;

            Vector3 pos = _playroomKit.GetPlayer(player.Key).GetState<Vector3>("pos");
            player.Value.GetComponent<Transform>().position = pos;
        }
    }

    public void OnClickedGrid(int x, int y)
    {
        if (gameOver)
        {
            Debug.LogWarning("Game Over");
            return;
        }

        if (_playroomKit.MyPlayer().id != currentTurnId)
        {
            Debug.Log("Not your turn!");
            return;
        }

        Vector2Int gridPos = new Vector2Int(x, y);
        if (occupiedCells.Contains(gridPos))
        {
            Debug.LogWarning("Grid is Already taken");
            return;
        }

        occupiedCells.Add(gridPos);

        string moveShape = (_playroomKit.MyPlayer().id == playerOne.id) ? "CROSS" : "CIRCLE";
        string data = $"{moveShape},{x},{y}";
        _playroomKit.RpcCall("SpawnShape", data, PlayroomKit.RpcMode.ALL);

        string nextTurn = (_playroomKit.MyPlayer().id == playerOne.id) ? playerTwo.id : playerOne.id;
        _playroomKit.RpcCall("SyncTurn", nextTurn, PlayroomKit.RpcMode.ALL);

    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }

    private void AddPlayer(PlayroomKit.Player player)
    {
        if (_playroomKit.IsHost())
        {
            playersIds.Add(player.id);
            string data = string.Join(",", playersIds);
            _playroomKit.RpcCall("UpdateLocalPlayers", data, PlayroomKit.RpcMode.OTHERS);
        }

        players.Add(player);

        playerJoined = true;
        player.OnQuit(RemovePlayer);


    }

    private static void RemovePlayer(string playerID)
    {
        if (PlayerDict.TryGetValue(playerID, out GameObject player))
        {
            PlayerDict.Remove(playerID);
            players.Remove(players.Find(p => p.id == playerID));
            playerGameObjects.Remove(player);
            Destroy(player);
        }
        else
        {
            Debug.LogWarning("Player is not in dictionary");
        }
    }

    public string GetMyPlayerId()
    {
        return _playroomKit.MyPlayer().id;
    }
    public string GetShapeType()
    {
        return shapeType;
    }
    public string GetPlayerOne()
    {
        return playerOne.id ?? "";
    }
    public string GetPlayerTwo()
    {
        return playerTwo.id ?? "";
    }
}
