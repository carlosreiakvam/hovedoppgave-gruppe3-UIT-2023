using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;
    public event EventHandler<OnChangeFocusEventArgs> OnChangeFocus;
    public class OnChangeFocusEventArgs : EventArgs
    {
        public bool IsChatActive;
    }
    //[SerializeField] private Button SendBtn;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private TextMeshProUGUI chatContent;
    private readonly List<string> _messages = new();
    private float buildDelay;
    private int maximumMessages = 10;

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
            DontDestroyOnLoad(gameObject.transform.parent);
        }
    }



    void Start()
    {
        chatContent.maxVisibleLines = maximumMessages;
        //SendBtn.onClick.AddListener(() => SubmitMsg());
        chatInput.onSubmit.AddListener(delegate { SubmitMsg(); });

    }

    void Update()
    {
        //if (NetworkManager.Singleton.IsConnectedClient)
        //{


        //    if (_messages.Count > _maximumMessages)
        //    {
        //        _messages.RemoveAt(0);
        //    }

        if (buildDelay < Time.time)
        {
            BuildChatContents();
            buildDelay = Time.time + 0.25f;
        }

        //Debug.Log("chat input is active");
        OnChangeFocus?.Invoke(this,
            new OnChangeFocusEventArgs { IsChatActive = chatInput.isFocused });

        //}
        //else if (_messages.Count > 0)
        //{
        //    _messages.Clear();
        //    chatContent.text = "0";
        //}
    }

    public void SubmitMsg()
    {
        //Security measures
        //string blankCheck = chatInput.text;
        //blankCheck = Regex.Replace(blankCheck, @"\s", "");
        //if (blankCheck == "")
        //{
        //    chatInput.ActivateInputField();
        //    chatInput.text = "";
        //    return;
        //}

        SendMsg(chatInput.text);
        chatInput.ActivateInputField();
        chatInput.text = "";
    }

    private void AddMsg(string message , ulong senderPlayerId)
    {
        
        _messages.Add($"Player {senderPlayerId}: { message }");

        if (_messages.Count > maximumMessages)
            _messages.RemoveAt(0);
    }

    public void SendMsg(string message)
    {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(string message , ServerRpcParams serverRpcParams = default)
    {
        ReceiveChatMessageClientRpc(message , serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string message , ulong senderPlayerId)
    {
        AddMsg(message, senderPlayerId);
    }

    void BuildChatContents()
    {
        string newContent = "";
        foreach (string s in _messages)
        {
            newContent += s + "\n";
        }

        chatContent.text = newContent;
    }

 
}
