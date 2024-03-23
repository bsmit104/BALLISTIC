# NetworkPlayer.cs
**Found in [/Player](../BALLISTIC/Assets/Scripts/Player/NetworkPlayer.cs)**

[Return to glossary](glossary.md)

>> **`public delegate void Notify()`**\
>> General event listener delegate.
>> 
> 
> ## `public class NetworkPlayer : NetworkBehaviour, IPlayerLeft`
> **Networked player controller, must be attached to the root game object of the player prefab.**
> 
> ### **Serialized Properties:**
>> **`private GameObject cmra`**\
>> The player's camera. Will be set active if the player instance is the local client. Should be deactivated by default.
> 
>> **`private float maxCmraDist`**\
>> The max distance the camera will be from the player.
> 
>> **`private float minCmraDist`**\
>> The min distance the camera will be from the player.
> 
>> **`private float cmraWallOffset`**\
>> How far the camera will sit off of the surface it is colliding with.
> 
>> **`private float cmraShoulderOffset`**\
>> How far the camera will swing out from the player as the camera is drawn in.
> 
>> **`private float walkSpeed`**\
>> The speed the player will walk at.
> 
>> **`private float crouchSpeed`**\
>> The speed the player will crouch walk at.
> 
>> **`private float sprintSpeed`**\
>> The speed the player will run at.
> 
>> **`private float jumpImpulse`**\
>> Controls jump height.
> 
>> **`private GroundedCollider grounded`**\
>> Collider script for checking if the player is grounded.
> 
>> **`private RagdollActivator ragdollActivator`**\
>> Script used to activate and deactivate the player's ragdoll. Should be attached to the hip joint.
> 
>> **`public Transform throwPoint`**\
>> Point from where the dodgeball is parented to when held.
> 
>> **`public float actionCooldown`**\
>> Cooldown duration between click inputs.
> 
>> **`private float aimDist`**\
>> The max distance aim target detection will be tested for.
> 
>> **`public DodgeballPickup pickupCollider`**\
>> The collider script used to determine what balls are near the player.
> 
> ### **Methods, Getters, and Setters:**
>> **`public static NetworkPlayer Local`**\
>> Get the NetworkPlayer instance assigned to the client.(e.g. returns a different instance depending on the computer it is run on).
>> 
> 
>> **`public PlayerRef GetRef`**\
>> Returns the PlayerRef associated with this player object.
>> 
> 
>> **`public bool IsAlive`**\
>> Returns true if the player is currently alive.
>> 
> 
>> **`public PlayerColor Color`**\
>> Returns the color assigned to this player.
>> 
> 
>> **`public float Sensitivity`**\
>> Set the mouse sensitivity for this player. Only meaningful when applied to the Local player.
>> 
> 
>> **`public bool IsHUDActive`**\
>> Returns true if the player's HUD is currently visible.
>> 
> 
>> **`public void SetHUDActive(bool state)`**\
>> Activate or deactivate the player's HUD.
>> 
> 
>> **`public RagdollActivator RagdollActivator`**\
>> Returns the ragdoll activator for this player.
>> 
> 
>> **`public Vector3 LookTarget`**\
>> Returns the global position of the point the player is looking at.
>> 
> 
>> **`public bool IsHoldingBall`**\
>> Returns true if the player is currently holding a ball.
>> 
> 
>> **`public void ActivatePlayerRagdoll()`**\
>> Synchronously activate player ragdoll across all clients.
>> 
> 
