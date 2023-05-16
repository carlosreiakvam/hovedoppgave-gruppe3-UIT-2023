using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// DoorForest class manages the behavior of the forest cave door in the game. 
/// It is a NetworkBehaviour, indicating it is expected to function in a networked multiplayer environment.
/// </summary>
public class CaveDoorForest : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    List<Vector2> spawnPositions;

    /// <summary>
    /// Called when the network spawns this object.
    /// Defines possible spawn positions and sets the forest cave door position.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        UpdateForestDoorPositionClientRpc(transform.position.x, transform.position.y);

    }

    [ClientRpc]
    private void UpdateForestDoorPositionClientRpc(float x, float y)
    {
        gameStatusSO.doorForestPosition = new Vector2(x, y);
    }


}
