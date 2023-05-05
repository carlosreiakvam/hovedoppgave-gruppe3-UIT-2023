using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveEntrance : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        bool relocateToCave = (gameObject.transform.position.x < 50);
        GameObject player = collision.transform.parent.gameObject;
        PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();

        playerBehaviour.RelocatePlayer(relocateToCave);
    }

}
