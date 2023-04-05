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


namespace HeroNetworkManager
{
    public class NetworkManager : MonoBehaviour
    {

        //[SerializeField] private UnityEvent relayCreated; // lets LobbyPreGame know when lobby is created
        public event EventHandler OnRelayCreated;  // lets LobbyPreGame know when lobby is created
        string relayJoinCode;
        public Allocation allocation;


        //public static NetworkManager _instance;
        //Singleton pattern: https://www.youtube.com/watch?v=2pCkInvkwZ0&t=125s
        public static NetworkManager Instance; //=> _instance;


        //private void Awake()
        //{
        //    if (_instance == null)
        //    {
        //        Debug.Log("creating NetworkManager for the first time");
        //        _instance = this;
        //        DontDestroyOnLoad(gameObject);
        //    }
        //    else
        //    {
        //        Debug.Log("destroying NetworkManager as it is already initialized");
        //        Destroy(gameObject);
        //    }
        //}

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log("destroying NetworkManager as it is already initialized");
                Destroy(gameObject);
                
            }
            else
            {
                Debug.Log("creating NetworkManager for the first time");
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }



        [Command]
        public async Task<Dictionary<string, string>> CreateRelay()
        {
            Debug.Log("CreateRelay");
            try
            {
                allocation = await RelayService.Instance.CreateAllocationAsync(3);
                relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new(allocation, "dtls");
                Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                Debug.Log("; JoinCode: " + relayJoinCode);
                //relayCreated.Invoke();
                OnRelayCreated?.Invoke(this, EventArgs.Empty);
                Unity.Netcode.NetworkManager.Singleton.StartHost();

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

        //public bool StartHost()
        //{
            
        //    //NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        //    //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;

        //    return Unity.Netcode.NetworkManager.Singleton.StartHost();
        //}



        //public void StartClient()
        //{
        //    //from KitchenGameMultiplayer
        //    //OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        //    //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        //    //NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        //    //Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        //    //Debug.Log($"RelayServerData: {relayServerData}");
        //    Debug.Log("In StartClient, JoinCode: " + relayJoinCode);
        //    //JoinRelay(relayJoinCode); //Does not jig
        //    //Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        //    Unity.Netcode.NetworkManager.Singleton.StartClient();
        //}


        [Command]
        public async void JoinRelay(string joinCode)
        {
            try
            {
                //relayJoinCode = joinCode; Just for testing
                Debug.Log("Joining Realy with " + joinCode);
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                //relayCreated.Invoke();
                OnRelayCreated?.Invoke(this, EventArgs.Empty);
                Debug.Log("Success on JoinRelay");
                Unity.Netcode.NetworkManager.Singleton.StartClient();
            }

            catch (RelayServiceException e)
            {
                Debug.Log("RelayServiceException: Fail on JoinRelay");
                Debug.Log(e);
            }
        }

    }

}