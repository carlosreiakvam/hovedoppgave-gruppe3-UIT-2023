using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        Light2D light2D = collision.transform.GetComponentInChildren<Light2D>();
        light2D.enabled = false;

        Vector2 relocateToPosition = gameStatusSO.caveDoorForest;
        relocateToPosition.y -= 2;
        playerBehaviour.RelocatePlayer(relocateToPosition);
    }

}
