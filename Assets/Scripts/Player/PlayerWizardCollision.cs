using QFSW.QC.Containers;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerWizardCollision : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.gameObject.tag == "Wizard")) { return; } // Return if not wizard
        if (!LocalPlayerManager.Singleton.localPlayer.playerHasRing) { Debug.Log("Player does not have ring"); return; }  // Return if player does not have ring

        ulong localPlayerId = LocalPlayerManager.Singleton.localPlayer.id;
        string playerName = LocalPlayerManager.Singleton.GetNameFromId(localPlayerId);

        OnPlayerWonGameServerRpc(localPlayerId, playerName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerWonGameServerRpc(ulong localPlayerId, string playerName)
    {
        Debug.LogWarning("PLAYERNAME: " + playerName);
        DeactivatePlayersClientRpc(localPlayerId);
        VisualizeGameWonClientRpc(playerName);

    }

    [ClientRpc]
    public void DeactivatePlayersClientRpc(ulong localPlayerId)
    {
        // Deactivate player. No despawn for networking reasons.
        GameManager.Singleton.DeactivateAllPlayers();
    }

    [ClientRpc]
    public void VisualizeGameWonClientRpc(string playerName)
    {
        // Deactivate player. No despawn for networking reasons.
        GameManager.Singleton.VisualizeOnGameWon(playerName);

    }


}
