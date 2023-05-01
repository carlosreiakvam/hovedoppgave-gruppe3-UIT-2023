using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class OpeningPort : MonoBehaviour
{
    GameManager gameManager;
    TextMeshPro wonText;
    GameObject gameUIGO;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        NetworkObject playerNetworkObject = collision.gameObject.GetComponentInParent<NetworkObject>();
        ulong playerNetworkObjectId = playerNetworkObject.NetworkObjectId;

        Debug.Log("Player with instance playerNetworkObjectId " + playerNetworkObjectId + " collided with opening port");

        Debug.Log("gamemanager says this person has ring: " + GameManager.Singleton.networkedPlayerIdHasRing);
        if (playerNetworkObjectId == GameManager.Singleton.networkedPlayerIdHasRing)
        {
            OnGameWonServerRpc(playerNetworkObjectId);
        }
    }

    [ServerRpc]
    public void OnGameWonServerRpc(ulong playerNetworkObjectId)
    {
        OnGameWonChangedClientRpc(playerNetworkObjectId);
    }

    [ClientRpc]
    private void OnGameWonChangedClientRpc(ulong playerNetworkObjectId)
    {
        GameObject gameUI = GameObject.Find("GameUI");
        TextMeshProUGUI infoText = gameUI.GetComponentInChildren<TextMeshProUGUI>();
        infoText.text = "GAME WON BY PLAYER with network id: " + playerNetworkObjectId;
    }


}
