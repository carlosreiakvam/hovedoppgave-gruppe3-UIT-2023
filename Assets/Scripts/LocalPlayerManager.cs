using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalPlayerManager : MonoBehaviour
{
    public struct LocalPlayer
    {
        public string name;
        public ulong id;
        public bool playerHasRing;
        public bool playerWonGame;
    }


    [SerializeField] public GameObject torchPowerup;
    [SerializeField] public GameObject ring;
    [SerializeField] public GameObject speedPowerup;

    [SerializeField] GameStatusSO gameStatusSO;
    public static LocalPlayerManager Singleton;
    public LocalPlayer localPlayer = new LocalPlayer();

    public void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);

        // PowerupUI
        torchPowerup.SetActive(false);
        speedPowerup.SetActive(false);
        ring.SetActive(false);
    }
    public bool IsThisIdForLocalPlayer(ulong id)
    {
        return localPlayer.id == id;
    }

    public string GetNameFromId(ulong id)
    {
        if (localPlayer.id == id) return localPlayer.name;
        else return "";
    }

    public void SetName(string name) { localPlayer.name = name; }
    public void SetId(ulong id) { localPlayer.id = id; }
    public void SetHasRing(bool playerHasRing) { localPlayer.playerHasRing = playerHasRing; }
    public void SetPlayerWonGame(bool playerWonGame) { localPlayer.playerWonGame = playerWonGame; }


    public void RegisterPlayerInScriptableObject()
    {
        string authId = AuthenticationService.Instance.PlayerId;
        ulong localClientid = NetworkManager.Singleton.LocalClientId;


        foreach (Player player in gameStatusSO.lobbyPlayers)
        {
            if (player.Data["PlayerId"].Value == authId)
            {
                Debug.LogWarning("FOUND PLAYER WITH NAME " + player.Data["PlayerName"].Value + " and client id " + localClientid);
                localPlayer.name = player.Data["PlayerName"].Value;
                localPlayer.id = localClientid;
            }
        }

    }

}
