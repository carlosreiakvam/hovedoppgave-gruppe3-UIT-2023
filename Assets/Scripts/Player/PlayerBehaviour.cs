using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : NetworkBehaviour
{
    /*TouchControls*/
    private CharacterController controller;
    private PlayerInput playerInput;

    [SerializeField] private GameObject playerAnimation;
    [SerializeField] private OwnerNetworkAnimator ownerNetworkAnimator;

    private readonly float boundX = 0.35f;
    private readonly float boundY = 0.17f;
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
    private bool woodenSword = true;

    private bool isRunningAndroid = false;
    private bool isRunningWindows = true; // set to false when testing on android

    private const string TOUCH_UI_TAG = "TouchUI";
    private void Initialize()
    {
        if (!IsOwner) return;
        mainCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        animator.SetFloat(PREVHORIZONTAL, 0);
        animator.SetFloat(PREVVERTICAL, -1);
        //transform.GetChild(1).gameObject.SetActive(false);
        //transform.GetChild(2).gameObject.SetActive(false);
        //transform.GetChild(3).gameObject.SetActive(false);
        //transform.GetChild(4).gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!ChatManager.Instance) return; //mulig det har med bypass å gjøre at denne er her. Fjern ved innlevering.
        ChatManager.Instance.OnChangeFocus += Toggle_PlayerControls; //subscribe

        if (!IsOwner) return;

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
            GameObject.FindWithTag(TOUCH_UI_TAG).SetActive(false);

        }
        else
        {
            Debug.Log("Current platform is not supported.");
        }

    }

    private void Toggle_PlayerControls(object sender, ChatManager.OnChangeFocusEventArgs e)
    {
        //Debug.Log("Game in focus? " + e.IsChatActive);

        chatInFocus = e.IsChatActive;


    }

    public override void OnNetworkSpawn()
    {
        transform.position = spawnPositionList[(int)OwnerClientId]; //OwnerClientId is not sequential, but can be handled in the Lobby (Multiplayer tutorial)
        Initialize();
        // TODO: merge bug on line bellow
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    private void Singleton_OnClientDisconnectCallback(ulong clientId) //worst practices? Use ServerRpcParams
    {
        if (clientId == OwnerClientId) //will warn that only server can despawn if server is shut down first.
            NetworkObject.Despawn();
    }

    private void LateUpdate()
    {
        if (IsLocalPlayer)
        {
            Vector3 pos = playerAnimation.transform.position;
            pos.z = -10;
            mainCamera.transform.position = pos;

            //Vector3 delta = Vector3.zero;

            ////check if we're inside the bounds of the x axis
            //float deltaX = transform.position.x - mainCamera.transform.position.x;


            //if (deltaX > boundX || deltaX < -boundX)
            //{
            //    if (mainCamera.transform.position.x < transform.position.x)
            //    {
            //        delta.x = deltaX - boundX;
            //    }
            //    else
            //    {
            //        delta.x = deltaX + boundX;
            //    }
            //}

            ////check if we're inside the bounds of the x axis
            //float deltaY = transform.position.y - mainCamera.transform.position.y;

            //if (deltaY > boundY || deltaY < -boundY)
            //{
            //    if (mainCamera.transform.position.y < transform.position.y)
            //    {
            //        delta.y = deltaY - boundY;
            //    }
            //    else
            //    {
            //        delta.y = deltaY + boundY;
            //    }
            //}

            //mainCamera.transform.position += new Vector3(delta.x, delta.y);
        }
    }

    void Update()
    {
        if (!chatInFocus)
        {
            if (isRunningAndroid)
                HandleTouchInput();

            if (isRunningWindows)
                HandleMovement();
        }
    }

    //void FixedUpdate()
    //{
    //    HandleInteractions();
    //}

    private void HandleInteractions()
    {
        if (!IsLocalPlayer) return;

        Vector2 inputVector = new Vector2(horizontalInput, verticalInput).normalized;
        Vector2 moveDir = new(inputVector.x, inputVector.y);

        if (moveDir != Vector2.zero)
            lastInteractDir = moveDir;

        float interactDistance = 0f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, interactDistance);

        if (hit.transform.TryGetComponent(out Enemy en))
            Debug.Log("Hit Something: " + en.tag);


    }

    private void HandleMovement()
    {
        if (!IsOwner) return;

        horizontalInput = Input.GetAxisRaw(HORIZONTAL);
        verticalInput = Input.GetAxisRaw(VERTICAL);

        //print("horizontalInput: " + horizontalInput.ToString());
        //print("verticalInput: " + verticalInput.ToString());

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
            print("E pressed");
            print("PREVHORIZONTAL: " + animator.GetFloat(PREVHORIZONTAL).ToString());
            print("PREVVERTICAL: " + animator.GetFloat(PREVVERTICAL).ToString());
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