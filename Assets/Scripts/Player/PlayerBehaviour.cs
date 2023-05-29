using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Rendering.Universal;

public class PlayerBehaviour : NetworkBehaviour
{
    public static PlayerBehaviour LocalInstance { get; private set; }

    private Coroutine currentSpeedCoroutine;
    private Coroutine currentTorchCoroutine;


    /*TouchControls*/
    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction attackAction;

    [SerializeField] GameStatusSO gamestatusSO;
    [SerializeField] private GameObject playerAnimation;
    [SerializeField] private OwnerNetworkAnimator ownerNetworkAnimator;
    [SerializeField] private List<Vector2> spawnPositionList;
    private bool isControlActive = false;

    private const float PLAYER_BASE_SPEED = 3;
    private const float PLAYER_INCREASED_SPEED = 6;
    private float playerSpeed = PLAYER_BASE_SPEED;

    private float horizontalInput;
    private float verticalInput;
    private Vector2 lastInteractDir;
    private LayerMask enemyLayerMask;
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

    private const string TOUCH_UI_TAG = "TouchUI";
    private const string TOUCH_UI_NAME = "TouchUIContainer";
    private PlayerHealth playerHealth;
    private void Initialize()
    {
        if (!IsOwner) return;
        animator = GetComponentInChildren<Animator>();
        animator.SetFloat(PREVHORIZONTAL, 0);
        animator.SetFloat(PREVVERTICAL, -1);

        playerHealth = GetComponentInChildren<PlayerHealth>();
        playerHealth.OnPlayerKnockdown += OnPlayerKnockdown;
    }
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

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        playerIsKnockedOut = e.isKnockedDown;
        playerHealth.OnPlayerKnockdown -= OnPlayerKnockdown;
        GameManager.Singleton.OnPlayerDeath();
    }

    private void Start()
    {
        ChatManager.Instance.OnChangeFocus += Toggle_PlayerControls;


    }


    private void Toggle_PlayerControls(object sender, ChatManager.OnChangeFocusEventArgs e)
    {
        chatInFocus = e.IsChatActive;
    }


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
        if (IsLocalPlayer) LocalInstance = this;
        Initialize();
    }



    public override void OnNetworkDespawn()
    {
        Debug.Log("OnNetworkDespawn i playerbehaviour");
        base.OnNetworkDespawn();
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (IsLocalPlayer)
        {
            Vector3 pos = playerAnimation.transform.position;
            pos.z = -10;
            Camera.main.transform.position = pos;
        }
    }

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

    public bool isAttackBtn;
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

    private void Attack()
    {
        if (woodenSword) { ownerNetworkAnimator.SetTrigger(WOODENATTACK); }
        else ownerNetworkAnimator.SetTrigger(STEELATTACK);
    }

    public void SetNewSword()
    {
        woodenSword = false;
    }

    public bool GetSword()
    {
        return woodenSword;
    }

    internal void IncreaseSpeed()
    {
        playerSpeed = PLAYER_INCREASED_SPEED;

        // If there is a coroutine running, stop it
        if (currentSpeedCoroutine != null) StopCoroutine(currentSpeedCoroutine);
        currentSpeedCoroutine = StartCoroutine(ResetSpeedAfterDelay(8));
    }


    IEnumerator ResetSpeedAfterDelay(float delay)
    {
        LocalPlayerManager.Singleton.speedPowerup.SetActive(true);
        yield return new WaitForSeconds(delay);
        LocalPlayerManager.Singleton.speedPowerup.SetActive(false);
        playerSpeed = PLAYER_BASE_SPEED;
    }
    internal void RelocatePlayer(Vector2 cavePosition)
    {
        playerAnimation.transform.position = cavePosition;
    }

    public void ControlActive(bool isActive)
    {
        if (!IsOwner) return;
        isControlActive = isActive;
    }


    internal void ActivateTorchPowerUp()
    {
        if (currentTorchCoroutine != null) StopCoroutine(currentTorchCoroutine);
        currentTorchCoroutine = StartCoroutine(FireUpNewTorch(8));
    }

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