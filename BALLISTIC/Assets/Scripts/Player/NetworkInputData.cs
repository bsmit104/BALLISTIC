using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// Describes the package that will be sent from client to host, communicating input data.
/// Set values in OnInput() method from NetworkPlayerManager,
/// Data is used to update the game state in FixedUpdateNetwork() method from NetworkPlayer.
/// </summary>
public struct NetworkInputData : INetworkInput
{
    public float horizontal;
    public float vertical;
    public bool sprintButtonPressed;
    public bool throwButtonPressed;
    public bool jumpButtonPressed;
    public bool crouchButtonPressed;

    public bool emote1ButtonPressed;
    public bool emote2ButtonPressed;
    public bool emote3ButtonPressed;
    public bool emote4ButtonPressed;

    public bool testButtonPressed;
}
