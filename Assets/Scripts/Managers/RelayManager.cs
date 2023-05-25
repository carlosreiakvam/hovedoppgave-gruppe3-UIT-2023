using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System;
using System.Collections.Generic;

public class RelayManager : MonoBehaviour
{
    public event EventHandler OnRelayCreated; // Subscribed by MenuManager
    string relayJoinCode;
    public Allocation allocation;


    public static RelayManager Singleton;
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);

        }
        else
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }


    /// <summary>
    /// Initializes Unity services and signs in anonymously.
    /// </summary>
    public async Task Authorize()
    {
        try
        {
            await InitAsyncWithProfile();
            if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e) { Debug.LogWarning(e); }
    }

    /// <summary>
    /// Initializes Unity services with a profile.
    /// </summary>
    /// <returns>True if initialization is successful, false otherwise.</returns>
    private async Task<bool> InitAsyncWithProfile()
    {
        int maxRetries = 5;
        int retries = 0;
        bool success = false;

        while (!success && retries < maxRetries)
        {
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

        return success;
    }





    /// <summary>
    /// Creates a relay allocation and starts the host.
    /// </summary>
    /// <returns>A dictionary containing the relay join code and allocation ID.</returns>
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
            { LobbyStringConst.RELAY_JOIN_CODE, relayJoinCode },
            { LobbyEnums.AllocationId.ToString(), allocation.AllocationId.ToString() }
        };
        return relayDict;
    }


    /// <summary>
    /// Joins a relay using the given join code.
    /// </summary>
    /// <param name="joinCode">The join code for the relay to join.</param>
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



    /// <summary>
    /// Joins a relay using the given join code (shortcut version).
    /// </summary>
    /// <param name="joinCode">The join code for the relay to join.</param>
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
