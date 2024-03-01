using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System;

[Serializable]
public struct PlayerColor
{
    public string colorName;
    public Material material;
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

    void OnDestroy()
    {
        foreach (var pair in spawnedPlayers)
        {
            if (pair.Value) Destroy(pair.Value.gameObject);
        }
    }

    // * ==================================================

    // * Spawning and Despawning ==========================

    [Tooltip("Prefab that will be instantiated for each player, this has the character controller")]
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [Tooltip("The colors mapped to each player. Length of this list defines the max number of players.")]
    [SerializeField] private PlayerColor[] playerColors;

    private Dictionary<PlayerRef, NetworkPlayer> spawnedPlayers = new Dictionary<PlayerRef, NetworkPlayer>();
    public Dictionary<PlayerRef, NetworkPlayer> Players { get { return spawnedPlayers; } }

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
        if (runner == null) Debug.Log("wtf");
        NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        NetworkPlayer netPlayer = networkPlayerObject.gameObject.GetComponent<NetworkPlayer>();

        // Keep track of the player avatars for easy access
        spawnedPlayers.Add(player, netPlayer);
        alivePlayers.Add(player);
        runner.SetPlayerObject(player, networkPlayerObject);

        return netPlayer;
    }

    /// <summary>
    /// Despawns the player, removing them from the lobby.
    /// </summary>
    /// <param name="player">The player who will be despawned.</param>
    public void DespawnPlayer(PlayerRef player)
    {
        if (runner.IsServer && spawnedPlayers.TryGetValue(player, out var netPlayer))
        {
            spawnedPlayers.Remove(player);
            if (alivePlayers.Contains(player))
            {
                alivePlayers.Remove(player);
            }
            runner.Despawn(netPlayer.GetComponent<NetworkObject>());
        }
    }

    /// <summary>
    /// Spawn a dummy player object.
    /// </summary>
    /// <returns>Dummy NetworkPlayer</returns>
    public NetworkPlayer GetDummy()
    {
        var obj = runner.Spawn(playerPrefab, Vector3.zero);
        obj.GetComponent<NetworkPlayer>().isDummy = true;
        return obj.GetComponent<NetworkPlayer>();
    }

    // * ==================================================

    // * Getting ==========================================

    private void AddPlayer(PlayerRef pRef, NetworkPlayer player)
    {
        spawnedPlayers.Add(pRef, player);
    }

    /// <summary>
    /// Get the NetworkPlayer linked to the given playerRef
    /// </summary>
    /// <param name="playerRef">Synchronized, unique player identifier</param>
    /// <returns>NetworkPlayer instance, or null if no matching player is found</returns>
    public NetworkPlayer GetPlayer(PlayerRef playerRef)
    {
        if (!spawnedPlayers.ContainsKey(playerRef))
        {
            if (runner.TryGetPlayerObject(playerRef, out var obj))
            {
                AddPlayer(playerRef, obj.GetComponent<NetworkPlayer>());
            }
            else
            {
                return null;
            }
        }
        return spawnedPlayers[playerRef];
    }

    public PlayerColor GetColor(PlayerRef player)
    {
        return playerColors[Mathf.Max(0, player.PlayerId - 1)];
    }

    // * ==================================================

    // * Track Living Players =============================

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
            alivePlayers.Remove(player);

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
            foreach (var pair in spawnedPlayers)
            {
                pair.Value.Reset();
                alivePlayers.Add(pair.Value.GetRef);
            }
        }
    }
}
