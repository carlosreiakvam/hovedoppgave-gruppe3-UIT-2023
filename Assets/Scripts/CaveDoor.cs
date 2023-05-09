using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class CaveDoor : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    List<Vector2> spawnPositions;
    private NetworkVariable<float> networkPositionX = new NetworkVariable<float>();
    private NetworkVariable<float> networkPositionY = new NetworkVariable<float>();
    bool isInForest = true;




    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isInForest = transform.position.x < 50f;
        if (isInForest) InitForestCaveDoor();
    }

    private void InitForestCaveDoor()
    {
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
            // For non servers aka clients.
            // They will get the same position from the network variables
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

        Vector2 relocateToPosition;
        if (isInForest)
        {
            relocateToPosition = gameStatusSO.caveDoorInCave;
            relocateToPosition.y -= 2;
            playerBehaviour.RelocatePlayer(relocateToPosition);
        }
        else
        {
            relocateToPosition = gameStatusSO.caveDoorForest;
            relocateToPosition.y -= 2;
            playerBehaviour.RelocatePlayer(relocateToPosition);

        }


    }

}
