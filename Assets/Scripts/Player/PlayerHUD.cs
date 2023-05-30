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
    [SerializeField] private List<PlayerNameSO> pName;

    public override void OnNetworkSpawn()
    {
        networkPlayerName.Value = (pName[(int)OwnerClientId]).Value;
        playerName.text = networkPlayerName.Value.ToString();
    }
}
