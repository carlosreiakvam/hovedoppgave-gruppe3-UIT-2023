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

/// <summary>
/// Manages the chat system in the game.
/// </summary>
public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;
    public event EventHandler<OnChangeFocusEventArgs> OnChangeFocus;

    [SerializeField] private List<PlayerNameSO> pName;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private TextMeshProUGUI chatContent;

    private readonly List<string> _messages = new();
    private float buildDelay;
    private int maximumMessages = 10;

    public class OnChangeFocusEventArgs : EventArgs
    {
        public bool IsChatActive;
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.parent);
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start()
    {
        chatContent.maxVisibleLines = maximumMessages;
        chatInput.onSubmit.AddListener(delegate { SubmitMsg(); });
    }

    /// <summary>
    /// Update is called every frame.
    /// </summary>
    void Update()
    {
        if (buildDelay < Time.time)
        {
            BuildChatContents();
            buildDelay = Time.time + 0.25f;
        }

        OnChangeFocus?.Invoke(this,
            new OnChangeFocusEventArgs { IsChatActive = chatInput.isFocused });
    }

    /// <summary>
    /// Submits the chat message entered by the player.
    /// </summary>
    public void SubmitMsg()
    {
        SendMsg(chatInput.text);
        chatInput.ActivateInputField();
        chatInput.text = "";
    }

    /// <summary>
    /// Adds a chat message to the message list.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="senderPlayerId">The ID of the player who sent the message.</param>
    /// <param name="senderName">The name of the player who sent the message.</param>
    private void AddMsg(string message, ulong senderPlayerId, string senderName = null)
    {
        if (senderName != null) _messages.Add($"{senderName}: {message}");
        else _messages.Add($"{pName[(int)senderPlayerId].Value}  : {message} ");

        if (_messages.Count > maximumMessages)
            _messages.RemoveAt(0);
    }

    /// <summary>
    /// Sends a chat message.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="senderName">The name of the player who is sending the message.</param>
    public void SendMsg(string message, string senderName = null)
    {
        SendChatMessageServerRpc(message, senderName, default);
    }


    /// <summary>
    /// Server RPC to send a chat message.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="senderName">The name of the player who is sending the message.</param>
    /// <param name="serverRpcParams">Server RPC parameters.</param>
    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(string message, string senderName = null, ServerRpcParams serverRpcParams = default)
    {
        ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId, senderName);
    }

    /// <summary>
    /// Client RPC to receive a chat message.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="senderPlayerId">The ID of the player who sent the message.</param>
    /// <param name="senderName">The name of the player who sent the message.</param>
    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string message, ulong senderPlayerId, string senderName = null)
    {
        AddMsg(message, senderPlayerId, senderName);
    }

    /// <summary>
    /// Builds the chat content from the messages list.
    /// </summary>
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
