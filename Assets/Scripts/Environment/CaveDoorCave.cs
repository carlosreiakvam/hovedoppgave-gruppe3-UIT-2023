using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
/// <summary>
/// DoorCave class manages the behavior of the cave door in the game. 
/// It is a NetworkBehaviour, indicating it is expected to function in a networked multiplayer environment.
/// </summary>
public class CaveDoorCave : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;

    /// <summary>
    /// Trigger event for the 2D Collider. If the Collider tag is "Player", it will trigger the relocation of the player.
    /// </summary>
    /// <param name="collision">The collider that triggered the event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        RelocatePlayer(collision);
    }

    /// <summary>
    /// Relocates the player to a new position defined in the game status scriptable object.
    /// Disables the player's light upon relocation.
    /// </summary>
    /// <param name="collision">The player's collider that triggered the relocation.</param>
    private void RelocatePlayer(Collider2D collision)
    {
        GameObject player = collision.transform.parent.gameObject;
        PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
        Light2D light2D = collision.transform.GetComponentInChildren<Light2D>();
        light2D.enabled = false;

        Vector2 relocateToPosition = gameStatusSO.doorForestPosition;
        relocateToPosition.y -= 2;
        playerBehaviour.RelocatePlayer(relocateToPosition);
    }
}
