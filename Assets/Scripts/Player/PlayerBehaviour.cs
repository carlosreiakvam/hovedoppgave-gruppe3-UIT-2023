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

    [SerializeField] GameStatusSO gamestatusSO;
    [SerializeField] private GameObject playerAnimation;
    [SerializeField] private OwnerNetworkAnimator ownerNetworkAnimator;

    //private readonly float boundX = 0.35f;
    //private readonly float boundY = 0.17f;
    [SerializeField] private FloatReference playerSpeed;
    [SerializeField] private List<Vector2> spawnPositionList;

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
        playerHealth.OnPlayerKnockdown += OnPlayerKnockdown; //subscribe
    }

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        playerIsKnockedOut = !playerIsKnockedOut;
    }

    private void Start()
    {
        ChatManager.Instance.OnChangeFocus += Toggle_PlayerControls; //subscribe

        if (gamestatusSO.isAndroid)
        {
            controller = gameObject.GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
        }
        else if (gamestatusSO.isWindows)
        {
            try { GameObject.FindWithTag(TOUCH_UI_TAG).SetActive(false); }
            catch { Debug.LogWarning("CANNOT FIND TouchUI"); }
        }
    }

    private void Toggle_PlayerControls(object sender, ChatManager.OnChangeFocusEventArgs e)
    {
        //Debug.Log("Game in focus? " + e.IsChatActive);

        chatInFocus = e.IsChatActive;


    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        transform.position = spawnPositionList[(int)OwnerClientId]; //OwnerClientId is not sequential, but can be handled in the Lobby (Multiplayer tutorial)
        Initialize(); //Running this is start might cause variables to not be initialized
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

        Vector2 movement = new Vector2(horizontalInput, verticalInput).normalized * (playerSpeed.Value * Time.deltaTime);
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            Attack();
        }
    }


    public void HandleTouchInput()
    {
        if (!IsOwner) return;

        Vector2 input = playerInput.actions["PlayerMovement"].ReadValue<Vector2>();
        Vector3 move = new(input.x, input.y);
        move = move.x * mainCamera.transform.right + move.y * mainCamera.transform.up;
        controller.Move(playerSpeed.Value * Time.deltaTime * move);

        animator.SetFloat(HORIZONTAL, input.x);
        animator.SetFloat(VERTICAL, input.y);
        animator.SetFloat(SPEED, new Vector2(input.x, input.y).normalized.sqrMagnitude);

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
}