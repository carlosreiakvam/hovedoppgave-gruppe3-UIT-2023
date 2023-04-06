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


public class RelayManager : MonoBehaviour
{
    public event EventHandler OnRelayCreated;  // lets LobbyRoom know when lobby is created
    string relayJoinCode;
    public Allocation allocation;


    //Singleton pattern: https://www.youtube.com/watch?v=2pCkInvkwZ0&t=125s
    public static RelayManager Instance;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("destroying RelayManager as it is already initialized");
            Destroy(gameObject);

        }
        else
        {
            Debug.Log("creating RelayManager for the first time");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }



    [Command]
    public async Task<Dictionary<string, string>> CreateRelay()
    {
        Debug.Log("CreateRelay in RelayManager");
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(3);
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            Debug.Log("; JoinCode: " + relayJoinCode);
            OnRelayCreated?.Invoke(this, EventArgs.Empty);
            NetworkManager.Singleton.StartHost();

        }
        catch (Exception e)
        {
            Debug.Log("Create Relay Error: " + e);
        }
        Dictionary<string, string> relayDict = new()
        {
            { LobbyEnums.RelayJoinCode.ToString(), relayJoinCode },
            { LobbyEnums.AllocationId.ToString(), allocation.AllocationId.ToString() }
        };
        return relayDict;
    }


    [Command]
    public async void JoinRelay(string joinCode)
    {
        try
        {
            //relayJoinCode = joinCode; Just for testing
            Debug.Log("Joining Realy with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            //relayCreated.Invoke();
            OnRelayCreated?.Invoke(this, EventArgs.Empty);
            Debug.Log("Success on JoinRelay");
            NetworkManager.Singleton.StartClient();
        }

        catch (RelayServiceException e)
        {
            Debug.Log("RelayServiceException: Fail on JoinRelay");
            Debug.Log(e);
        }
    }

}
