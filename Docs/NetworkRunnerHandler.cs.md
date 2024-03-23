# NetworkRunnerHandler.cs
**Found in [/Managers](../BALLISTIC/Assets/Scripts/Managers/NetworkRunnerHandler.cs)**

[Return to glossary](Glossary.md)

> ## `public class NetworkRunnerHandler : MonoBehaviour`
> **Spawns the NetworkRunner to start the online lobby.OnHost(), and OnClient() methods are main interface for starting the game.**
> 
> ### **Serialized Properties:**
>> **`private NetworkRunner networkRunnerPrefab`**\
>> Empty game object with the NetworkRunner and NetworkPlayerManager scripts attached.
> 
>> **`private GameObject loadingPopupPrefab`**\
>> Loading popup prefab spawned when loading into a game.
> 
>> **`private int lobbyCodeDigits`**\
>> The number of digits that a lobby name will have
> 
>> **`private int lobbyScene`**\
>> The build index of the scene to transition to on host/join. Find in build settings.
> 
> ### **Methods, Getters, and Setters:**
>> **`public string LobbyName`**\
>> The lobby name/code for the currently joined game.Will return an empty string if not in a lobby.
>> 
> 
>> **`public void OnHost()`**\
>> Start a game as a host, uses a random number (turned to string) as the lobby name?
>> 
> 
>> **`public void OnClient()`**\
>> Joins an existing game as a client, fails if a fetched lobby name is not found.
>> 
> 
