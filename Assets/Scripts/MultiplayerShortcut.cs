using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerShortcut : MonoBehaviour
{
    [Command]
    public async void HostShortcut()
    {
        await RelayManager.Instance.InitAndAuthorize();
        await RelayManager.Instance.CreateRelayShortcut();
    }
    [Command]
    public void LoadGame()
    { LoadNetwork(); }

    [Command]
    public async void ClientShortcut(string joincode)
    {
        await RelayManager.Instance.InitAndAuthorize();
        await RelayManager.Instance.JoinRelayShortcut(joincode);
    }

    private void LoadNetwork()
    {
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
    private void NetworkManager_ConnectionApprovalCallback(
     NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
     NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        connectionApprovalResponse.Approved = true;
    }
}
