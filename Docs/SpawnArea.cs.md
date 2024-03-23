# SpawnArea.cs
**Found in [/SpawnArea/Scripts](../BALLISTIC/Assets/Scripts/SpawnArea/Scripts/SpawnArea.cs)**

[Return to glossary](glossary.md)

> ## `public class SpawnArea : MonoBehaviour`
> **A 2D mesh that represents a valid spawn area.Drawn using in-editor tool.**
> 
> ### **Serialized Properties:**
>> **`public float handleRadius = 0.15f`**\
>> The radius of point handles.
> 
>> **`public float lineWidth = 4f`**\
>> Width of lines connecting points.
> 
>> **`public float lineSensitivity = 0.1f`**\
>> How close the mouse needs to be to a line to click it.
> 
> ### **Methods, Getters, and Setters:**
>> **`public Bounds GetBounds`**\
>> Returns a bounds encapsulating the shape defined by points.
>> 
> 
>> **`public Vector3 GetPoint(int index)`**\
>> Returns the global space coords for the given point.
>> 
>> **Arguments:**\
>> *index:* The index of the point.
> 
>> **`public void SetPoint(int index, Vector3 position)`**\
>> Sets the point at the given index to the given global position. The position will be translated toa local position.
>> 
>> **Arguments:**\
>> *index:* index number for the points list.\
>> *position:* The global space position.
> 
>> **`public void InsertPoint(int index, Vector3 position)`**\
>> Inserts the point at the given index to the given global position. The position will be translated toa local position.
>> 
>> **Arguments:**\
>> *index:* index number for the points list.\
>> *position:* The global space position.
> 
>> **`public void RemovePoint(int index)`**\
>> Removes a point from the given index.
>> 
> 
>> **`public float Height`**\
>> The current Y position of the plane.
>> 
> 
>> **`public void GenerateMesh()`**\
>> Updates the spawn area's mesh to match the current shape defined by points.
>> 
> 
>> **`public Vector3 GetRandomPosition()`**\
>> Returns a position inside of the area defined by points.If the area is not valid, then it will return Vector3.zero.
>> 
>>
>>**Returns:** The random position within this area.
> 
