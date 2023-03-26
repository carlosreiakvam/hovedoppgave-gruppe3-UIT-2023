using UnityEngine;
using Unity.Netcode;

public class PlayerAnimationID : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private readonly string PLAYER_ID_STRING = "Player_Id";
    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            SetPlayerIdServerRpc();
        }
        //animator.SetInteger("Player_Id", (int) OwnerClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerIdClientRpc(serverRpcParams.Receive.SenderClientId);
    }
    [ClientRpc]
    private void SetPlayerIdClientRpc(ulong clientId)
    {
        animator.SetInteger(PLAYER_ID_STRING, (int)clientId);
    }

}
