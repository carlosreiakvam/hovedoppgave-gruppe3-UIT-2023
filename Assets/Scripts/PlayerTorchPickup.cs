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

        NetworkObject playerNetworkObject = GetComponentInParent<NetworkObject>();
        ulong playerNetworkObjectId = playerNetworkObject.NetworkObjectId;
        TorchPowerupOnServerRpc(playerNetworkObjectId);
        ulong torchNetworkId = collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        DestroyTorchServerRpc(torchNetworkId);

        //SpawnManager.Singleton.DespawnObjectServerRpc(NetworkObject.NetworkObjectId);
    }


    [ServerRpc(RequireOwnership = false)]
    public void TorchPowerupOnServerRpc(ulong id)
    {
        if (!IsServer) return;
        TorchPowerupOnClientRpc(id);
    }

    [ServerRpc]
    private void DestroyTorchServerRpc(ulong networkId)
    {
        NetworkObject networkObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out networkObject))
        {
            networkObject.Despawn();
        }
        else
        {
            Debug.Log($"No NetworkObject with NetworkId {networkId} found.");
        }

    }

    [ClientRpc]
    public void TorchPowerupOnClientRpc(ulong id)
    {

        // Find the player that entered the cave
        var spawnedObjects = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList;
        foreach (var obj in spawnedObjects)
        {
            if (obj.NetworkObjectId == id)
            {
                Light2D light = obj.GetComponentInChildren<Light2D>();

                if (currentTorchCoroutine != null) StopCoroutine(currentTorchCoroutine);
                currentTorchCoroutine = StartCoroutine(FireUpNewTorch(light, 8));

            }
        }
    }

    internal IEnumerator FireUpNewTorch(Light2D playerLight, float delay)
    {
        playerLight.pointLightOuterRadius = 10;

        LocalPlayerManager.Singleton.torchPowerup.SetActive(true);
        yield return new WaitForSeconds(delay);
        LocalPlayerManager.Singleton.torchPowerup.SetActive(false);

        playerLight.pointLightOuterRadius = 5;
    }

}
