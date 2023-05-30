using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerTorchPickup : NetworkBehaviour
{
    private Coroutine currentTorchCoroutine;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Torch") return;

        // Get players network object id 
        NetworkObject playerNetworkObject = GetComponentInParent<NetworkObject>();
        ulong playerNetworkObjectId = playerNetworkObject.NetworkObjectId;

        // Tell server about torch pickup
        TorchPowerupOnServerRpc(playerNetworkObjectId);

        // Destroy torch
        ulong torchNetworkId = collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        DestroyTorchServerRpc(torchNetworkId);
    }


    [ServerRpc(RequireOwnership = false)]
    public void TorchPowerupOnServerRpc(ulong playerNetworkObjectId)
    {
        if (!IsServer) return;

        // Tell clients about torch pickup
        TorchPowerupOnClientRpc(playerNetworkObjectId);
    }


    [ClientRpc]
    public void TorchPowerupOnClientRpc(ulong playerNetworkId)
    {

        // Find the player that entered the cave
        var spawnedObjects = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList;
        foreach (var obj in spawnedObjects)
        {
            if (obj.NetworkObjectId == playerNetworkId)
            {
                if (currentTorchCoroutine != null) StopCoroutine(currentTorchCoroutine);
                currentTorchCoroutine = StartCoroutine(FireUpNewTorch(obj, 8));
            }
        }
    }

    internal IEnumerator FireUpNewTorch(NetworkObject player, float delay)
    {
        // Get player light
        Light2D playerLight = player.GetComponentInChildren<Light2D>();

        // Turn on torch and ui for owner of torch
        if (player.IsOwner) LocalPlayerManager.Singleton.torchPowerupUI.SetActive(true);
        playerLight.pointLightOuterRadius = 10;

        // Wait for delay
        yield return new WaitForSeconds(delay);

        // Turn off torch and ui for owner of torch
        playerLight.pointLightOuterRadius = 5;
        if (player.IsOwner) LocalPlayerManager.Singleton.torchPowerupUI.SetActive(false);
    }

    [ServerRpc]
    private void DestroyTorchServerRpc(ulong torchNetworkId)
    {
        NetworkObject networkObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(torchNetworkId, out networkObject))
        {
            networkObject.Despawn();
        }
        else
        {
            Debug.Log($"No NetworkObject with NetworkId {torchNetworkId} found.");
        }
    }
}
