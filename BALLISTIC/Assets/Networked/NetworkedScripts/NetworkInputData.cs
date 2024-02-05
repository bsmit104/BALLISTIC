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
    // TODO: fill with needed data for input transport
}
