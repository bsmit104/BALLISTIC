# RadialMenu.cs
**Found in [/Menus](../BALLISTIC/Assets/Scripts/Menus/RadialMenu.cs)**

[Return to glossary](Glossary.md)


> ## `public class RadialMenu : MonoBehaviour`
> **Use to procedurally create a radial menu interface.**
> 
> ### **Serialized Properties:**
>> **`private TextMeshProUGUI menuElement`**\
>> The prefab instantiated for each menu item.
> 
>> **`public event OptionSelection OnOptionSelected`**\
>> Event triggered when the menu is closed with an option selected.
> 
> ### **Methods, Getters, and Setters:**
>> **`public bool IsOpen`**\
>> Returns true if the menu is currently open.
>> 
> 
>> **`public void OpenMenu()`**\
>> Opens the radial menu.
>> 
> 
>> **`public void CloseMenu()`**\
>> Closes the radial menu.
>> 
> 
>> **`public void AddOption(string option)`**\
>> Adds a new option to the menu.
>> 
> 
>> **`public string CurrentOption`**\
>> Returns the current option the player is hovering over.If the player isn't hovering over anything, returns an empty string.
>> 
> 

