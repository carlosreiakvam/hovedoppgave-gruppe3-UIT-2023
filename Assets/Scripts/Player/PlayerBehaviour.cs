using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : NetworkBehaviour
{
    public static PlayerBehaviour LocalInstance { get; private set; }

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
    private const float PLAYER_INCREASED_SPEED = 10;
    private float playerSpeed = PLAYER_BASE_SPEED;

    private Camera mainCamera;
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

    private bool isRunningAndroid = false;
    private bool isRunningWindows = false;

    private const string TOUCH_UI_TAG = "TouchUI";
    private PlayerHealth playerHealth;
    private void Initialize()
    {
        if (!IsOwner) return;
        mainCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        animator.SetFloat(PREVHORIZONTAL, 0);
        animator.SetFloat(PREVVERTICAL, -1);

        playerHealth = GetComponentInChildren<PlayerHealth>();
        playerHealth.OnPlayerKnockdown += OnPlayerKnockdown;
    }

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        playerIsKnockedOut = e.isKnockedDown;
        playerHealth.OnPlayerKnockdown -= OnPlayerKnockdown;
    }

    private void Start()
    {
        ChatManager.Instance.OnChangeFocus += Toggle_PlayerControls;

        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Current platform is Android.");
            isRunningAndroid = true;
            controller = gameObject.GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor ||
                 Application.platform == RuntimePlatform.WindowsPlayer)
        {
            Debug.Log("Current platform is Windows.");
            isRunningWindows = true;

            try { GameObject.FindWithTag(TOUCH_UI_TAG).SetActive(false); }
            catch (Exception e) { Debug.Log("TOUCH_UI_TAG not found because it was already inactive"); }
        }

        else
        {
            Debug.Log("Current platform is not supported.");
        }
    }

    private void Toggle_PlayerControls(object sender, ChatManager.OnChangeFocusEventArgs e)
    {
        chatInFocus = e.IsChatActive;
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

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
        if (!isControlActive) return;
        if (playerIsKnockedOut) return;

        if (!chatInFocus)
        {
            if (isRunningAndroid)
                HandleTouchInput();

            if (isRunningWindows)
                HandleMovement();
        }
    }


    private void HandleInteractions()
    {
        if (!IsLocalPlayer) return;

        Vector2 inputVector = new Vector2(horizontalInput, verticalInput).normalized;
        Vector2 moveDir = new(inputVector.x, inputVector.y);

        if (moveDir != Vector2.zero)
            lastInteractDir = moveDir;

        float interactDistance = 0f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, interactDistance);

        /*        if (hit.transform.TryGetComponent(out Enemy en))
                    Debug.Log("Hit Something: " + en.tag);
        */

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
        move = move.x * mainCamera.transform.right + move.y * mainCamera.transform.up;
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
        Debug.LogWarning("Increasing speed for player");
        playerSpeed = 6;
        StartCoroutine(ResetSpeedAfterDelay(10));
    }


    IEnumerator ResetSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerSpeed = 4;
        Debug.LogWarning("Resetting speed for player");
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

}