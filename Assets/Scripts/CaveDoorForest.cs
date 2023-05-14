using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveDoorForest : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    List<Vector2> spawnPositions;
    private NetworkVariable<float> networkPositionX = new NetworkVariable<float>();
    private NetworkVariable<float> networkPositionY = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        spawnPositions = new List<Vector2>
    {
        new Vector2(10, 9.6f),
        new Vector2(10, 23.6f),
        new Vector2(44.5f, 50.6f),
        new Vector2(3.5f, 50.6f),
        new Vector2(8, 35.6f),
        new Vector2(37, 37.6f),
        new Vector2(41, 18.5f),
        new Vector2(44, 10.6f),
        new Vector2(46.6f, 18.6f)
    };

        Vector2 caveDoorForestPosition;
        if (IsServer)
        {
            // Get random position from list
            caveDoorForestPosition = spawnPositions[Random.Range(0, spawnPositions.Count)];

            // Set network variables
            networkPositionX.Value = caveDoorForestPosition.x;
            networkPositionY.Value = caveDoorForestPosition.y;
        }
        else
        {
            //   clients will get  position from the network variables
            caveDoorForestPosition = new Vector2(networkPositionX.Value, networkPositionY.Value);
        }

        // Relocate forest cavedoor
        gameStatusSO.caveDoorForest = caveDoorForestPosition;
        transform.position = caveDoorForestPosition;
    }


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

        Vector2 relocateToPosition = gameStatusSO.caveDoorInCave;
        relocateToPosition.y += 2;
        playerBehaviour.RelocatePlayer(relocateToPosition);
        light2D.enabled = true;

    }

}
