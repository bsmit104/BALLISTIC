# NetworkPlayerManager.cs
**Found in [/Managers](../BALLISTIC/Assets/Scripts/Managers/NetworkPlayerManager.cs)**

[Return to glossary](Glossary.md)


> ## `public struct PlayerColor`
> **Serialized struct that contains all of the color info for each player.**
> 
> ### **Serialized Properties:**
>> **`public string colorName`**\
>> The name of the color displayed as the player's name.
> 
>> **`public Material material`**\
>> The material used by the player's model.
> 
>> **`public Color color`**\
>> The color that can be used for any other purposes.
> 
>> **`public Sprite icon`**\
>> The player icon of the colored robot's head.
> 

> ## `public class NetworkPlayerManager : MonoBehaviour`
> **Manages instances of NetworkPlayers, each joined client will have a NetworkPlayer assigned to represent them in-game.**
> 
> ### **Serialized Properties:**
>> **`private NetworkPrefabRef playerPrefab`**\
>> Prefab that will be instantiated for each player, this has the character controller
> 
>> **`private PlayerColor[] playerColors`**\
>> The colors mapped to each player. Length of this list defines the max number of players.
> 
> ### **Methods, Getters, and Setters:**
>> **`public static NetworkPlayerManager Instance`**\
>> Returns the singleton instance of the local NetworkPlayerManager.
>> 
> 
>> **`public void Init(NetworkRunner runner, NetworkLevelManager levelManager)`**\
>> Should only be called by NetworkRunnerCallbacks.
>> 
> 
>> **`public Dictionary<PlayerRef, NetworkPlayer> Players`**\
>> Get the map of players currently in the game. DO NOT MUTATE.
>> 
> 
>> **`public int PlayerCount`**\
>> Returns the number of players currently in the lobby.
>> 
> 
>> **`public int MaxPlayerCount`**\
>> The max number of players allowed in a lobby.
>> 
> 
>> **`public bool LobbyHasSpace`**\
>> Returns true if the current player count is below the max player count.
>> 
> 
>> **`public NetworkPlayer SpawnPlayer(PlayerRef player)`**\
>> Spawns a new player in the lobby, gives them a random valid position.
>> 
>> **Arguments:**\
>> *player:* The PlayerRef that will be attached to the player.
> 
>> **`public void DespawnPlayer(PlayerRef player)`**\
>> Despawns the player, removing them from the lobby.
>> 
>> **Arguments:**\
>> *player:* The player who will be despawned.
> 
>> **`public NetworkPlayer GetDummy()`**\
>> Spawn a dummy player object.
>> 
>>
>>**Returns:** Dummy NetworkPlayer
> 
>> **`public NetworkPlayer GetPlayer(PlayerRef playerRef)`**\
>> Get the NetworkPlayer linked to the given playerRef
>> 
>> **Arguments:**\
>> *playerRef:* Synchronized, unique player identifier
>>
>>**Returns:** NetworkPlayer instance, or null if no matching player is found
> 
>> **`public PlayerColor GetColor(PlayerRef player)`**\
>> Get the PlayerColor associated with the given player.This will loop back to the first color if the end of the list is reached.
>> 
>> **Arguments:**\
>> *player:* The PlayerRef for the specific player.
>>
>>**Returns:** The PlayerColor, which is NOT a Color struct.
> 
>> **`public void PlayerDied(PlayerRef player)`**\
>> Notify the player manager of a player dying. Will call LevelManager.DeclareWinner()when only one player is left.
>> 
>> **Arguments:**\
>> *player:* The PlayerRef to the player that died.
> 
>> **`public void ResetPlayers()`**\
>> Reset all players to their alive state, and add them back to the living players list.
>> 
> 

