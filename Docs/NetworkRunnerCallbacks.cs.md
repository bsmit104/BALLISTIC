# NetworkRunnerCallbacks.cs
**Found in [/Managers](../BALLISTIC/Assets/Scripts/Managers/NetworkRunnerCallbacks.cs)**

[Return to glossary](Glossary.md)


> ## `public class NetworkRunnerCallbacks : MonoBehaviour, INetworkRunnerCallbacks`
> **Implements all network events, and initializes managers.**
> 
> ### **Serialized Properties:**
>> **`private GameObject joinLeaveCanvas`**\
>> The canvas used to display player join & leave events.
> 
>> **`private TextMeshProUGUI joinLeaveText`**\
>> The text attached to the joinLeaveCanvas used to display which player joined or left.
> 
>> **`private float joinLeaveDisplayTime`**\
>> The duration the join/leave UI will be displayed for.
> 
>> **`private GameObject PauseCanvas`**\
>> The canvas that contains the pause menu.
> 
>> **`private TextMeshProUGUI lobbyCodeText`**\
>> The text on the pause menu used to display the lobby code.
> 
>> **`ConnectionPopup networkPopupPrefab`**\
>> The popup that will be instantiated when the NetworkRunner is shutdown.
> 
> ### **Methods, Getters, and Setters:**
>> **`public static NetworkRunnerCallbacks Instance`**\
>> The singleton instance of the NetworkRunnerCallbacks.
>> 
> 
>> **`public bool NotInitialized`**\
>> Returns true if managers have not been initialized yet.
>> 
> 
>> **`public bool IsPaused`**\
>> Returns true if the local player has paused their game.
>> 
> 
>> **`public void Unpause()`**\
>> Unpause the local player.
>> 
> 
>> **`public void LeaveGame()`**\
>> Leave the current lobby. If the player is the host, then the lobby will be shutdown,and all players will be disconnected.
>> 
> 

