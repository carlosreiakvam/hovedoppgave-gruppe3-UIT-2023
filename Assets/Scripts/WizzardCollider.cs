using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class WizzardCollider : MonoBehaviour
{
/*    GameManager gameManager;
    TextMeshPro wonText;
    GameObject gameUIGO;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        NetworkObject playerNetworkObject = collision.gameObject.GetComponentInParent<NetworkObject>();
        ulong playerNetworkObjectId = playerNetworkObject.NetworkObjectId;

        Debug.Log("Player with instance playerNetworkObjectId " + playerNetworkObjectId + " collided with wizzard");

        Debug.Log("gamemanager says this person has ring: " + GameManager.Singleton.networkedPlayerIdHasRing);
        if (playerNetworkObjectId == GameManager.Singleton.networkedPlayerIdHasRing)
        {
            OnGameWonServerRpc(playerNetworkObjectId);
        }
    }

    [ServerRpc]
    public void OnGameWonServerRpc(string playerNetworkObjectId)
    {
        OnGameWonChangedClientRpc(playerNetworkObjectId);
    }

    [ClientRpc]
    private void OnGameWonChangedClientRpc(ulong playerNetworkObjectId)
    {
        GameManager.Singleton.OnGameWon(playerNetworkObjectId);
    }

*/}
