using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerCaveCollision : NetworkBehaviour
{

    [SerializeField] GameStatusSO gamestatusSO;
    const string DOOR_FOREST_TAG = "DoorForest";
    const string DOOR_CAVE_TAG = "DoorCave";



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.tag == DOOR_FOREST_TAG || collision.tag == DOOR_CAVE_TAG)) return;

        PlayerBehaviour playerBehaviour = GetComponentInParent<PlayerBehaviour>();
        Vector2 doorPosition;

        NetworkObject playerNetworkObject = GetComponentInParent<NetworkObject>();
        ulong playerNetworkObjectId = playerNetworkObject.NetworkObjectId;

        if (collision.CompareTag(DOOR_FOREST_TAG))
        {
            doorPosition = gamestatusSO.doorCavePosition;
            doorPosition.y += 2;
            playerBehaviour.RelocatePlayer(doorPosition);
            LightPlayerServerRpc(playerNetworkObjectId, true);
        }
        else if (collision.CompareTag(DOOR_CAVE_TAG))
        {
            doorPosition = gamestatusSO.doorCavePosition;
            doorPosition = gamestatusSO.doorForestPosition;
            doorPosition.y -= 2;
            playerBehaviour.RelocatePlayer(doorPosition);
            LightPlayerServerRpc(playerNetworkObjectId, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LightPlayerServerRpc(ulong id, bool isLit)
    {
        if (!IsServer) return;
        LightPlayerClientRpc(id, isLit);
    }

    [ClientRpc]
    public void LightPlayerClientRpc(ulong id, bool isLit)
    {
        Light2D light;

        // Find the player that entered the cave
        var spawnedObjects = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList;
        foreach (var obj in spawnedObjects)
        {
            if (obj.NetworkObjectId == id)
            {
                light = obj.GetComponentInChildren<Light2D>();
                light.enabled = isLit;
            }
        }
    }
}
