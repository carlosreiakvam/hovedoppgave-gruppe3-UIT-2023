using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class CaveDoorCave : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        RelocatePlayer(collision);
    }

    private void RelocatePlayer(Collider2D collision)
    {
        GameObject player = collision.transform.parent.gameObject;
        PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();

        Vector2 relocateToPosition = gameStatusSO.caveDoorForest;
        relocateToPosition.y -= 2;
        playerBehaviour.RelocatePlayer(relocateToPosition);
    }

}
