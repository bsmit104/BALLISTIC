# NetworkLevelManager.cs
**Found in [/Managers](../BALLISTIC/Assets/Scripts/Managers/NetworkLevelManager.cs)**

[Return to glossary](glossary.md)

> ## `public class NetworkLevelManager : MonoBehaviour`
> **Manages scene transitions between levels.Tracks how many players are alive to know when to end the game, and then move on to the next level.**
> 
> ### **Serialized Properties:**
>> **`private GameObject lobbyCanvas`**\
>> The canvas which will display the lobby code at the start of the game.
> 
>> **`private TextMeshProUGUI lobbyCodeText`**\
>> The text attached to the lobbyCanvas used to display the lobby code.
> 
>> **`private int lobbySceneIndex`**\
>> The build index of the lobby scene
> 
>> **`private int firstLevelIndex`**\
>> The next level will be picked based on a range of scene build indices. Each level should be placed in a row.
> 
>> **`private int lastLevelIndex`**\
>> The next level will be picked based on a range of scene build indices. Each level should be placed in a row.
> 
>> **``**\
>> The level manager will track which levels have been played. Refreshing will reset this list.
> 
>> **`private GameObject transitionCanvas`**\
>> The canvas used for displaying transitions.
> 
>> **`RectTransform transitionElement`**\
>> The UI element used to create the transition.
> 
>> **`float enterTransitionDuration`**\
>> Duration for transition into a scene.
> 
>> **`float exitTransitionDuration`**\
>> Duration for transition out of a scene.
> 
>> **`float waitBetweenTransitions`**\
>> Hold on the last frame of the transition to prevent it from being too disorienting.
> 
>> **``**\
>> Frequency in seconds for checking if the next level has loaded.
> 
>> **`private int ballsPerLevel // TEMP: Hardcoded`**\
>> Fixed number of balls to spawn per level.
> 
>> **`float waitBeforeWinScreen`**\
>> Pause after last player dies before going to the win screen.
> 
>> **`float winScreenDuration`**\
>> How long the winner will be displayed before going to the next level.
> 
>> **`private GameObject winScreen`**\
>> What screen is displayed when the round is over.
> 
>> **`private TextMeshProUGUI winText`**\
>> Displays the name of the winner.
> 
>> **`private GameObject remoteWinText`**\
>> Displays the remote player's name that won.
> 
>> **`private GameObject localWinText`**\
>> Displays 'YOURE THE #1 BALLER'
> 
> ### **Methods, Getters, and Setters:**
>> **`public static NetworkLevelManager Instance`**\
>> Get the global level manager instance.
>> 
> 
>> **`public static NetworkRunner Runner`**\
>> The local network runner.
>> 
> 
>> **`public void Init(NetworkRunner runner, NetworkPlayerManager players, NetworkBallManager balls)`**\
>> Initializes the level manager, should only be called once when the NetworkRunnerPrefab is created.
>> 
> 
>> **`public bool IsAtLobby`**\
>> Returns true if the current scene loaded in the lobby waiting room.
>> 
> 
>> **`public bool IsInLevel`**\
>> Returns true if the current scene loaded is one of the levels.
>> 
> 
>> **`public int GetRandomLevel()`**\
>> Gets a random scene build index for a level.Avoids revisiting levels, which can be controlled with the "refreshChance" attribute.
>> 
>>
>>**Returns:** A scene build index.
> 
>> **`public void SetTransitionCanvasActive(bool state)`**\
>> Wrapper around GameObject.SetActive().
>> 
> 
>> **`public bool ExitRunning`**\
>> Returns true if the exit transition is playing.
>> 
> 
>> **`public bool EnterRunning`**\
>> Returns true if the enter transition is playing.
>> 
> 
>> **`public bool LevelChangeRunning`**\
>> Returns true if the level is currently being switched.
>> 
> 
>> **`public void ClientLoaded()`**\
>> Notify the level manager that a player has finished loading into the next level.
>> 
> 
>> **`public bool AllClientsLoaded`**\
>> Returns true if all players have fished loading into the next level.
>> 
> 
>> **`public bool LocalLevelLoaded`**\
>> Returns true if the local player has finished loading into the next level.
>> 
> 
>> **`public void GoToLevel(int buildIndex)`**\
>> Synchronizes scene transitions between clients.Activates transition animations for unloading and loading the scene.Scene change only happens on host instance, and is then synchronized.
>> 
>> **Arguments:**\
>> *buildIndex:* The build index for the level.
> 
>> **`public bool IsResetting`**\
>> Returns true if the level is currently being reset/prepared for play.
>> 
> 
>> **`public void ResetLevel()`**\
>> Resets the current level to be prepared for play.Places players and balls into scene.
>> 
> 
>> **`public bool WinSequenceRunning`**\
>> Returns true if the winner is currently being displayed.
>> 
> 
>> **`public void DeclareWinner(PlayerRef player)`**\
>> Set a winner player, and transition to the winning screen.Should be called by player manager when only 1 player is left alive.
>> 
>> **Arguments:**\
>> *player:* The winning player
> 
> ## `public class LevelManagerMessages : SimulationBehaviour`
> **Message broker for NetworkLevelManager.**
> 
> ### **Methods, Getters, and Setters:**
>> **`[Rpc]
public static void RPC_DeclareWinner(NetworkRunner runner, PlayerRef player)`**\
>> Called by the host to notify clients who the winner is, and trigger their winner sequence.
>> 
> 
>> **`[Rpc]
public static void RPC_GoToLevel(NetworkRunner runner, int buildIndex)`**\
>> Called by host to tell clients to transition to the next level.
>> 
> 
>> **`[Rpc]
public static void RPC_ClientHasLoaded(NetworkRunner runner)`**\
>> Called by clients to tell the host that they've finished loading into the next level.
>> 
> 
>> **`[Rpc]
public static void RPC_EnterLevel(NetworkRunner runner)`**\
>> Called by the host to tell clients to enter into the next level to resume play.
>> 
> 
