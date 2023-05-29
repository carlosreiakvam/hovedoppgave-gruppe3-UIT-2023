using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Represents the behavior of a player in the game.
/// </summary>
public class PlayerBehaviour : NetworkBehaviour
{
    private Coroutine currentSpeedCoroutine;
    private Coroutine currentTorchCoroutine;

    private CharacterController controller;
    private PlayerInput playerInput;

    [SerializeField] GameStatusSO gamestatusSO;
    [SerializeField] private GameObject playerAnimation;
    [SerializeField] private OwnerNetworkAnimator ownerNetworkAnimator;
    [SerializeField] private List<Vector2> spawnPositionList;

    private const float PLAYER_BASE_SPEED = 3;
    private const float PLAYER_INCREASED_SPEED = 6;
    private float playerSpeed = PLAYER_BASE_SPEED;

    private float horizontalInput;
    private float verticalInput;
    private Animator animator;

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string SPEED = "Speed";
    private const string PREVHORIZONTAL = "PrevHorizontal";
    private const string PREVVERTICAL = "PrevVertical";
    private const string WOODENATTACK = "WoodenAttack";
    private const string STEELATTACK = "SteelAttack";

    private bool chatInFocus = false;
    private bool playerIsKnockedOut = false;
    private bool woodenSword = true;

    private const string TOUCH_UI_NAME = "TouchUIContainer";
    private PlayerHealth playerHealth;

    /// <summary>
    /// Initializes the player behavior.
    /// </summary>
    private void Initialize()
    {
        if (!IsOwner) return;

        animator = GetComponentInChildren<Animator>();
        animator.SetFloat(PREVHORIZONTAL, 0);
        animator.SetFloat(PREVVERTICAL, -1);

        playerHealth = GetComponentInChildren<PlayerHealth>();
        playerHealth.OnPlayerKnockdown += OnPlayerKnockdown;
    }

    /// <summary>
    /// Runs before every other method
    /// </summary>
    private void Awake()
    {
        try
        {
            GameObject touchUIContainer = GameObject.Find(TOUCH_UI_NAME);
            Canvas touchUICanvas = touchUIContainer.GetComponent<Canvas>();
            touchUICanvas.enabled = false;
        }
        catch { }
    }

    /// <summary>
    /// Callback method. Tells what's going to happen if the player is knocked down.
    /// </summary>
    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        playerIsKnockedOut = e.isKnockedDown;
        playerHealth.OnPlayerKnockdown -= OnPlayerKnockdown;
        GameManager.Singleton.OnPlayerDeath();
    }

    /// <summary>
    /// Runs after OnNetworkSpawn (because this script belongs to a dynamically spawned object)
    /// </summary>
    private void Start()
    {
        ChatManager.Instance.OnChangeFocus += Toggle_PlayerControls;
    }

    /// <summary>
    /// Toggles player controls based on the focus of the chat.
    /// </summary>
    private void Toggle_PlayerControls(object sender, ChatManager.OnChangeFocusEventArgs e)
    {
        chatInFocus = e.IsChatActive;
    }

    /// <summary>
    /// Called when the player is spawned in the network. After Awake()
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (gamestatusSO.isAndroid)
        {
            controller = gameObject.GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
        }

        base.OnNetworkSpawn();

        Light2D playerLight = GetComponentInChildren<Light2D>();
        playerLight.enabled = false;
        transform.position = spawnPositionList[(int)OwnerClientId];
        Initialize();
    }

    /// <summary>
    /// Called when the player is despawned from the network.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        Debug.Log("OnNetworkDespawn i playerbehaviour");
        base.OnNetworkDespawn();
        Destroy(gameObject);
    }

    /// <summary>
    /// Makes sure movement is done before updating camera position
    /// </summary>
    private void LateUpdate()
    {
        if (IsLocalPlayer)
        {
            Vector3 pos = playerAnimation.transform.position;
            pos.z = -10;
            Camera.main.transform.position = pos;
        }
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    void Update()
    {
        if (playerIsKnockedOut) return;

        if (!chatInFocus)
        {
            if (gamestatusSO.isAndroid)
                HandleTouchInput();

            if (gamestatusSO.isWindows)
                HandleMovement();
        }
    }

    /// <summary>
    /// Handles player movement on Windows platform.
    /// </summary>
    private void HandleMovement()
    {
        if (!IsOwner) return;

        horizontalInput = Input.GetAxisRaw(HORIZONTAL);
        verticalInput = Input.GetAxisRaw(VERTICAL);

        Vector2 movement = new Vector2(horizontalInput, verticalInput).normalized * (playerSpeed * Time.deltaTime);
        transform.position += (Vector3)movement;

        animator.SetFloat(HORIZONTAL, horizontalInput);
        animator.SetFloat(VERTICAL, verticalInput);
        animator.SetFloat(SPEED, new Vector2(horizontalInput, verticalInput).normalized.sqrMagnitude);

        // if player is walking, update last horizontal and vertical values for animator
        if (horizontalInput != 0 || verticalInput != 0)
        {
            animator.SetFloat(PREVHORIZONTAL, horizontalInput);
            animator.SetFloat(PREVVERTICAL, verticalInput);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    /// <summary>
    /// Handles player input on Android platform.
    /// </summary>
    public void HandleTouchInput()
    {
        if (!IsOwner) return;

        Vector2 input = playerInput.actions["PlayerMovement"].ReadValue<Vector2>();
        Vector3 move = new(input.x, input.y);
        move = move.x * Camera.main.transform.right + move.y * Camera.main.transform.up;
        controller.Move(playerSpeed * Time.deltaTime * move);

        animator.SetFloat(HORIZONTAL, input.x);
        animator.SetFloat(VERTICAL, input.y);
        animator.SetFloat(SPEED, new Vector2(input.x, input.y).normalized.sqrMagnitude);

        if (horizontalInput != 0 || verticalInput != 0)
        {
            animator.SetFloat(PREVHORIZONTAL, horizontalInput);
            animator.SetFloat(PREVVERTICAL, verticalInput);
        }

        bool attack = Convert.ToBoolean(playerInput.actions["PlayerAttack"].ReadValue<float>());
        if (attack)
            Attack();
    }

    /// <summary>
    /// Performs an attack action.
    /// </summary>
    private void Attack()
    {
        if (woodenSword) { ownerNetworkAnimator.SetTrigger(WOODENATTACK); }
        else ownerNetworkAnimator.SetTrigger(STEELATTACK);
    }

    /// <summary>
    /// Sets the player to have a new sword.
    /// </summary>
    public void SetNewSword()
    {
        woodenSword = false;
    }

    /// <summary>
    /// Returns whether the player has a wooden sword or not.
    /// </summary>
    public bool GetSword()
    {
        return woodenSword;
    }

    /// <summary>
    /// Increases the player's speed.
    /// </summary>
    internal void IncreaseSpeed()
    {
        playerSpeed = PLAYER_INCREASED_SPEED;

        // If there is a coroutine running, stop it
        if (currentSpeedCoroutine != null) StopCoroutine(currentSpeedCoroutine);
        currentSpeedCoroutine = StartCoroutine(ResetSpeedAfterDelay(8));
    }

    /// <summary>
    /// Resets the player's speed after a specified delay.
    /// </summary>
    IEnumerator ResetSpeedAfterDelay(float delay)
    {
        LocalPlayerManager.Singleton.speedPowerup.SetActive(true);
        yield return new WaitForSeconds(delay);
        LocalPlayerManager.Singleton.speedPowerup.SetActive(false);
        playerSpeed = PLAYER_BASE_SPEED;
    }

    /// <summary>
    /// Relocates the player to a new position.
    /// </summary>
    internal void RelocatePlayer(Vector2 cavePosition)
    {
        playerAnimation.transform.position = cavePosition;
    }

    /// <summary>
    /// Activates the torch power-up for the player.
    /// </summary>
    internal void ActivateTorchPowerUp()
    {
        if (currentTorchCoroutine != null) StopCoroutine(currentTorchCoroutine);
        currentTorchCoroutine = StartCoroutine(FireUpNewTorch(8));
    }

    /// <summary>
    /// Activates the torch power-up for a specified duration.
    /// </summary>
    internal IEnumerator FireUpNewTorch(float delay)
    {
        Light2D playerLight = GetComponentInChildren<Light2D>();
        playerLight.pointLightOuterRadius = 10;

        LocalPlayerManager.Singleton.torchPowerup.SetActive(true);
        yield return new WaitForSeconds(delay);
        LocalPlayerManager.Singleton.torchPowerup.SetActive(false);

        playerLight.pointLightOuterRadius = 5;
    }
}
