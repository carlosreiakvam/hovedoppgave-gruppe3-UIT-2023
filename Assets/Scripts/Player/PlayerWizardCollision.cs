using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerWizardCollision : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.gameObject.tag == "Wizard")) { return; } // Return if not wizard
        if (!LocalPlayerManager.Singleton.localPlayer.playerHasRing) { Debug.Log("Player does not have ring"); return; }  // Return if player does not have ring

        string localPlayerName = LocalPlayerManager.Singleton.localPlayer.name;
        ulong localPlayerId = LocalPlayerManager.Singleton.localPlayer.id;

        OnPlayerWonGameServerRpc(localPlayerName, localPlayerId);
    }


    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerWonGameServerRpc(string name, ulong id)
    {
        OnPlayerWonGameClientRpc(name, id);
    }

    [ClientRpc]
    public void OnPlayerWonGameClientRpc(string name, ulong networkObjectId)
    {
        SpawnManager.Singleton.DespawnAllPlayers();
        GameManager.Singleton.VisualizeOnGameWon(name);
    }

}
