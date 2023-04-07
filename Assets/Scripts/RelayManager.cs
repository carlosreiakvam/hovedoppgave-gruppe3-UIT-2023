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



    public async Task InitAndAuthorize()
    {
        int maxRetries = 5;
        int retries = 0;
        bool success = false;

        while (!success && retries < maxRetries)
        {
            // Init async with profile. Attempts to get a unique profilename
            try
            {
                var options = new InitializationOptions();
                string profileName = "profilename" + UnityEngine.Random.Range(0, 10000);
                options.SetProfile(profileName);
                await UnityServices.InitializeAsync(options);
                success = true;
            }
            catch (Exception) { retries++; Debug.Log("retrying another profilename"); }
        }

        // Authorize
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e) { Debug.LogWarning(e); }
    }


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
            Debug.Log("Joining Realy with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
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

    public async Task<Dictionary<LobbyEnums, string>> CreateRelayShortcut()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(3);
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            Debug.Log("; JoinCode: " + relayJoinCode);
            NetworkManager.Singleton.StartHost();

        }
        catch (Exception e)
        {
            Debug.Log("Create Relay Error: " + e);
        }
        Dictionary<LobbyEnums, string> relayDict = new()
        {
            { LobbyEnums.RelayJoinCode, relayJoinCode },
            { LobbyEnums.AllocationId, allocation.AllocationId.ToString() }
        };
        return relayDict;
    }

    public async Task JoinRelayShortcut(string joinCode)
    {
        try
        {
            joinCode = joinCode.Substring(0, 6); // ensure 6 characters only
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            Debug.Log("Success on JoinRelay");
        }

        catch (RelayServiceException e)
        {
            Debug.Log("RelayServiceException: Fail on JoinRelay");
            Debug.Log(e);
        }
        NetworkManager.Singleton.StartClient();
    }


}
