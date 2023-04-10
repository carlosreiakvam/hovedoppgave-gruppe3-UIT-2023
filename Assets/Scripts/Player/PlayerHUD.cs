using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    private readonly NetworkVariable<FixedString128Bytes> networkPlayerName = new("Player: 0", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
    //[SerializeField] private Animator animator;
    [SerializeField] private List<PlayerNameSO> pName;

    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
       foreach(PlayerNameSO p in pName)
        {
            Debug.Log("playersearch:" + p.Value);
        }
        Debug.Log("playerName:" + (pName[(int)OwnerClientId]).Value);
        networkPlayerName.Value = (pName[(int)OwnerClientId]).Value;
        Debug.Log("networkName: " + networkPlayerName.Value);
        //networkPlayerName.Value = "Player: " + (OwnerClientId + 1);
        playerName.text = networkPlayerName.Value.ToString();
    //    animator.SetInteger("Player_Id", (int) OwnerClientId);
    }

}
