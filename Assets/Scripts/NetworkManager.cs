using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using QFSW.QC;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviour
{
    //Singleton pattern: https://www.youtube.com/watch?v=2pCkInvkwZ0&t=125s
    [SerializeField] private UnityEvent relayCreated; // lets LobbyPreGame know when lobby is created
    string joinCode;
    public Allocation allocation;

    public static NetworkManager _instance;
    public static NetworkManager Instance => _instance;


    private void Awake()
    {
        if (_instance == null)
        {
            Debug.Log("creating NetworkManager for the first time");
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("destroying NetworkManager as it is already initialized");
            Destroy(gameObject);
        }
    }



    [Command]
    public async Task<Dictionary<string, string>> CreateRelay()
    {
        Debug.Log("CreateRelay");
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(3);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new(allocation, "dtls");
            Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            Debug.Log("; JoinCode: " + joinCode);
            relayCreated.Invoke();

        }
        catch (Exception e)
        {
            Debug.Log("Create Relay Error: " + e);
        }
        Dictionary<string, string> relayDict = new()
        {
            { LobbyEnums.RelayJoinCode.ToString(), joinCode },
            { LobbyEnums.AllocationId.ToString(), allocation.AllocationId.ToString() }
        };
        return relayDict;
    }

    public bool StartHost() 
    {
        return Unity.Netcode.NetworkManager.Singleton.StartHost(); 
    }

    public bool StartClient() 
    { 
        return Unity.Netcode.NetworkManager.Singleton.StartClient(); 
    }


    [Command]
    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Realy with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            relayCreated.Invoke();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

}