# LoadingPopup.cs
**Found in [/Menus](../BALLISTIC/Assets/Scripts/Menus/LoadingPopup.cs)**

[Return to glossary](Glossary.md)

> ## `public class LoadingPopup : MonoBehaviour`
> **Instantiated to show players the game is currently loading something.Only instantiate this if loading consistently takes a long time, and it will result in a scene change.**
> 
> ### **Serialized Properties:**
>> **`private TextMeshProUGUI text`**\
>> Displays the animated loading text.
> 
>> **`private float dotDuration`**\
>> The duration between dots '.' being added to text.
> 
>> **`private int dotCount`**\
>> The max number of dots '.' that will be added before looping back to 0.
> 
