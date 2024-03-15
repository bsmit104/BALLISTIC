using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System;

/// <summary>
/// Serialized struct that contains all of the color info for each player.
/// </summary>
[Serializable]
public struct PlayerColor
{
    /// <summary>
    /// The name of the color displayed as the player's name.
    /// </summary>
    public string colorName;
    /// <summary>
    /// The material used by the player's model.
    /// </summary>
    public Material material;
    /// <summary>
    /// The color that can be used for any other purposes.
    /// </summary>
    public Color color;
    /// <summary>
    /// The player icon of the colored robot's head.
    /// </summary>
    public Sprite icon;
}

/// <summary>
/// Manages instances of NetworkPlayers, each joined client will have a NetworkPlayer assigned to represent them in-game.
/// </summary>
public class NetworkPlayerManager : MonoBehaviour
{
    /// <summary>
    /// Returns the singleton instance of the local NetworkPlayerManager.
    /// </summary>
    public static NetworkPlayerManager Instance { get { return _instance; } }
    private static NetworkPlayerManager _instance = null;

    // * Managers =========================================

    private NetworkRunner runner;
    private NetworkLevelManager levelManager;

    /// <summary>
    /// Should only be called by NetworkRunnerCallbacks.
    /// </summary>
    public void Init(NetworkRunner runner, NetworkLevelManager levelManager)
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("NetworkPlayerManager singleton instantiated twice");
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (MaxPlayerCount > playerColors.Length)
        {
            Debug.LogError("There are not enough colors for every player!");
        }

        this.runner = runner;
        this.levelManager = levelManager;
    }

    // * ==================================================

    // * Spawning and Despawning ==========================

    [Tooltip("Prefab that will be instantiated for each player, this has the character controller")]
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [Tooltip("The colors mapped to each player. Length of this list defines the max number of players.")]
    [SerializeField] private PlayerColor[] playerColors;

    /// <summary>
    /// Get the map of players currently in the game. DO NOT MUTATE.
    /// </summary>
    public Dictionary<PlayerRef, NetworkPlayer> Players { get { return spawnedPlayers; } }
    private Dictionary<PlayerRef, NetworkPlayer> spawnedPlayers = new Dictionary<PlayerRef, NetworkPlayer>();

    /// <summary>
    /// Returns the number of players currently in the lobby.
    /// </summary>
    public int PlayerCount { get { return spawnedPlayers.Count; } }

    /// <summary>
    /// The max number of players allowed in a lobby.
    /// </summary>
    public int MaxPlayerCount { get { return playerColors.Length; } }

    /// <summary>
    /// Returns true if the current player count is below the max player count.
    /// </summary>
    public bool LobbyHasSpace { get { return PlayerCount < MaxPlayerCount; } }

    /// <summary>
    /// Spawns a new player in the lobby, gives them a random valid position.
    /// </summary>
    /// <param name="player">The PlayerRef that will be attached to the player.</param>
    /// <returns></returns>
    public NetworkPlayer SpawnPlayer(PlayerRef player)
    {
        // Create a unique position for the player
        Vector3 spawnPosition = Spawner.GetSpawnPoint();
        NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        NetworkPlayer netPlayer = networkPlayerObject.gameObject.GetComponent<NetworkPlayer>();

        // Keep track of the player avatars for easy access
        SetPlayer(player, netPlayer);
        runner.SetPlayerObject(player, networkPlayerObject);

        return netPlayer;
    }

    /// <summary>
    /// Despawns the player, removing them from the lobby.
    /// </summary>
    /// <param name="player">The player who will be despawned.</param>
    public void DespawnPlayer(PlayerRef player)
    {
        if (spawnedPlayers.TryGetValue(player, out var netPlayer))
        {
            spawnedPlayers.Remove(player);
            RemoveFromPlayerList(player);
            if (alivePlayers.Contains(player))
            {
                alivePlayers.Remove(player);
            }
            if (runner.IsServer) runner.Despawn(netPlayer.GetComponent<NetworkObject>());
        }
    }

    /// <summary>
    /// Spawn a dummy player object.
    /// </summary>
    /// <returns>Dummy NetworkPlayer</returns>
    public NetworkPlayer GetDummy()
    {
        var obj = runner.Spawn(playerPrefab, Vector3.zero);
        obj.GetComponent<NetworkPlayer>().IsDummy = true;
        return obj.GetComponent<NetworkPlayer>();
    }

    // * ==================================================

    // * Getting & Setting ================================

    public void SetPlayer(PlayerRef playerRef, NetworkPlayer player)
    {
        if (spawnedPlayers.ContainsKey(playerRef)) return;

        spawnedPlayers[playerRef] = player;
        AddToPlayerList(playerRef);
    }

    /// <summary>
    /// Get the NetworkPlayer linked to the given playerRef
    /// </summary>
    /// <param name="playerRef">Synchronized, unique player identifier</param>
    /// <returns>NetworkPlayer instance, or null if no matching player is found</returns>
    public NetworkPlayer GetPlayer(PlayerRef playerRef)
    {
        // Player objects will need to be retrieved from the Runner on clients
        if (!spawnedPlayers.ContainsKey(playerRef))
        {
            if (runner.TryGetPlayerObject(playerRef, out var obj))
            {
                spawnedPlayers.Add(playerRef, obj.GetComponent<NetworkPlayer>());
            }
            else
            {
                return null;
            }
        }
        return spawnedPlayers[playerRef];
    }

    /// <summary>
    /// Get the PlayerColor associated with the given player.
    /// This will loop back to the first color if the end of the list is reached.
    /// </summary>
    /// <param name="player">The PlayerRef for the specific player.</param>
    /// <returns>The PlayerColor, which is NOT a Color struct.</returns>
    public PlayerColor GetColor(PlayerRef player)
    {
        return playerColors[Mathf.Max(0, player.PlayerId - 1) % playerColors.Length];
    }

    // * ==================================================

    // * Track Living Players =============================

    // Tracks the currently alive players
    private List<PlayerRef> alivePlayers = new List<PlayerRef>();

    /// <summary>
    /// Notify the player manager of a player dying. Will call LevelManager.DeclareWinner() 
    /// when only one player is left.
    /// </summary>
    /// <param name="player">The PlayerRef to the player that died.</param>
    public void PlayerDied(PlayerRef player)
    {
        if (alivePlayers.Contains(player))
        {
            // Remove player after they die
            alivePlayers.Remove(player);

            // If no one is left, declare the winner
            if (alivePlayers.Count == 1 && spawnedPlayers.Count > 1)
            {
                levelManager.DeclareWinner(alivePlayers[0]);
            }
        }
    }

    // * ==================================================

    // * Reset ============================================

    /// <summary>
    /// Reset all players to their alive state, and add them back to the living players list.
    /// </summary>
    public void ResetPlayers()
    {
        if (runner.IsServer)
        {
            alivePlayers.Clear();
            // All players drop the ball they're holding before reset
            // Ensures players whose ragdolls are being held reset properly
            foreach (var pair in spawnedPlayers)
            {
                pair.Value.DropBall();
            }
            // Reset each player. Calls a reset RPC on each player
            foreach (var pair in spawnedPlayers)
            {
                pair.Value.Reset();
                alivePlayers.Add(pair.Value.GetRef);
            }
        }
    }

    // * ==================================================

    // * UI ===============================================

    [Space]
    [Header("Player List & Quick Chat")]
    [SerializeField] private RectTransform playerListMenu;
    [SerializeField] private float hiddenX;
    [SerializeField] private float displayX;
    [SerializeField] private float moveSpeed;
    [SerializeField] private PlayerListElement playerListElementPrefab;

    private Dictionary<PlayerRef, PlayerListElement> playerListElements = new Dictionary<PlayerRef, PlayerListElement>();

    private void AddToPlayerList(PlayerRef playerRef)
    {
        var playerElem = Instantiate(playerListElementPrefab, playerListMenu.transform.GetChild(1));
        playerElem.SetPlayer(playerRef);
        playerListElements.Add(playerRef, playerElem);
    }

    private void RemoveFromPlayerList(PlayerRef playerRef)
    {
        if (playerListElements.TryGetValue(playerRef, out var playerElem))
        {
            Destroy(playerElem.gameObject);
            playerListElements.Remove(playerRef);
        }
    }

    private Coroutine movePlayerList;
    
    public bool PlayerListDisplayed { get { return playerListDisplayed; } }
    private bool playerListDisplayed;


    public void DisplayPlayerList()
    {
        if (movePlayerList != null)
        {
            StopCoroutine(movePlayerList);
        }
        playerListDisplayed = true;
        movePlayerList = StartCoroutine(MovePlayerList(displayX));
    }

    public void HidePlayerList()
    {
        if (movePlayerList != null)
        {
            StopCoroutine(movePlayerList);
        }
        playerListDisplayed = false;
        movePlayerList = StartCoroutine(MovePlayerList(hiddenX));
    }

    IEnumerator MovePlayerList(float pos)
    {
        float timer = Mathf.Abs(playerListMenu.anchoredPosition.x - pos) / moveSpeed;
        float dir = pos - playerListMenu.anchoredPosition.x > 0 ? 1 : -1;
        while (timer > 0)
        {
            playerListMenu.anchoredPosition += new Vector2(dir * moveSpeed * Time.deltaTime, 0);
            timer -= Time.deltaTime;
            yield return null;
        }
        playerListMenu.anchoredPosition = new Vector2(pos, playerListMenu.anchoredPosition.y);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PlayerListDisplayed)
            {
                HidePlayerList();
            }
            else
            {
                DisplayPlayerList();
            }
        }
    }

    // * ==================================================
}
