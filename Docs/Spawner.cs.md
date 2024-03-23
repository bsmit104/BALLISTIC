# Spawner.cs
**Found in [/SpawnArea/Scripts](../BALLISTIC/Assets/Scripts/SpawnArea/Scripts/Spawner.cs)**

[Return to glossary](Glossary.md)

> ## `public class Spawner : MonoBehaviour`
> **Singleton which should exist on every level. Used to get valid spawn positions.Provides an editor interface for level designers to define valid spawn areas.**
> 
> ### **Serialized Properties:**
>> **`private bool displayAreaOnPlay = false`**\
>> When enabled, spawn areas will be displayed in the game.
> 
> ### **Methods, Getters, and Setters:**
>> **`public static Vector3 GetSpawnPoint()`**\
>> Gets a random, valid spawn position for the current level.
>> 
>>
>>**Returns:** The spawn position in global space.
> 
>> **`public static Vector3 GetSpawnPoint(Bounds bounds)`**\
>> Gets a random, valid spawn position for the current level.Accounts for a given bounds to place objects on the floor cleanly.Assumes the object's origin is in the center of the bounds.
>> 
>> **Arguments:**\
>> *bounds:* The bounds of the object this spawn position will be used on.
>>
>>**Returns:** The spawn position in global space.
> 
