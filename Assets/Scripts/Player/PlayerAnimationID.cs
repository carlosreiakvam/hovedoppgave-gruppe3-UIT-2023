using UnityEngine;
using Unity.Netcode;

public class PlayerAnimationID : NetworkBehaviour
{
    [SerializeField] private Animator animator;

    public override void OnNetworkSpawn()
    {
        animator.SetInteger("Player_Id", (int) OwnerClientId);
    }

}
