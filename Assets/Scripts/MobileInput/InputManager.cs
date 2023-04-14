using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    //[SerializeField] private FloatReference playerSpeed;
    private TouchControls touchControls;
    private float playerSpeed = 4;
    //private CharacterController controller;
    //private PlayerInput playerInput;
    //private InputAction attackAction; //touchposition
    //private InputAction movementAction; //touchpress
    //private const string HORIZONTAL_INPUT = "Horizontal";
    //private const string VERTICAL_INPUT = "Vertical";
    //private const string SPEED = "Speed";
    //PlayerBehaviour playerBehaviour;

    private void Awake()
    {
        //playerBehaviour = player.GetComponent<PlayerBehaviour>();
        /****************Pregenerated player input component*************************************/

        //playerInput = GetComponent<PlayerInput>();

        //playerInput.onActionTriggered += PlayerInput_onActionTriggered; //regular keyboard WASD controls 

        //movementAction = playerInput.actions["PlayerMovement"];
        //attackAction = playerInput.actions["PlayerAttack"];

        /************************************************************************************/

        /****************Pregenerated C# sharp script *************************************/
        touchControls = new TouchControls();
        touchControls.Touch.PlayerMovement.performed += Movement_Performed;
        //touchControls.Touch.PlayerAttack.performed += PlayerInput_onActionTriggered;
        /***************************************************ÆÆÆ*********************************/


    }

    private void Start()
    {
        //    subscribe when pressing down and pass context to StartTouch()
        //    touchControls.Touch.PlayerAttack.started += ctx => StartTouch(ctx);
        //    touchControls.Touch.PlayerAttack.canceled += ctx => EndTouch(ctx);
        //controller = gameObject.GetComponent<CharacterController>();
    }

    //private void Update()
    //{

    //    Vector2 move = touchControls.Touch.PlayerMovement.ReadValue<Vector2>();
    //    player.transform.position += (Vector3)move * playerSpeed.Value;

    //}

    //private void PlayerInput_onActionTriggered(InputAction.CallbackContext context) //the attack
    //{
    //    Debug.Log(context);
    //}

    private void Movement_Performed(InputAction.CallbackContext context)
    {

        //Vector3 position = Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>());

        //position.z = 0;

        //playerBehaviour.HandleTouchInput(position);
        Debug.Log(context);
        //Debug.Log("player.transform.position: " + player.transform.position);
        Vector2 move = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
        player.transform.position += (Vector3) move * playerSpeed;
    }

    private void OnEnable() //alternative to using update function, using events
    {
        touchControls.Enable();
        //movementAction.performed += TouchPressed;
    }

    private void OnDisable() //disable the event
    {
        touchControls.Disable();
        //movementAction.performed -= TouchPressed;
    }

    //context returns info about the touch
    //private void StartTouch(InputAction.CallbackContext context) //pressed down on screen
    //{

    //    Debug.Log("Touch started " + touchControls.Touch.PlayerAttack.ReadValue<float>());

    //}

    //private void EndTouch(InputAction.CallbackContext context) //when stop pressing on screen.
    //{
    //    Debug.Log("Touch ended");
    //}

    //private void TouchPressed(InputAction.CallbackContext context)
    //{
    //    Debug.Log("TouchPressed");

    //   playerBehaviour.HandleTouchInput(playerInput, controller);
    //    //Vector2 position = Camera.main.ScreenToWorldPoint(attackAction.ReadValue<Vector2>());  //Convert screen space to world space
    //    //player.transform.position = position;
    //    //position.z = player.transform.position.z;

    //    //xPos = context.ReadValue<Vector2>().x;
    //    //yPos = context.ReadValue<Vector2>().y;

    //    //Vector2 movement = new Vector2(xPos, yPos).normalized * (/*playerSpeed.Value*/4 * Time.deltaTime); ;
    //    //player.transform.position += (Vector3)movement;

    //    //Debug.Log("TouchPressed" + context.ReadValue<Vector2>());
    //    //Vector2 position = Camera.main.ScreenToWorldPoint(movementAction.ReadValue<Vector2>());  //Convert screen space to world space
    //    //player.transform.position = position;
    //    //player.transform.position = position;
    //    Debug.Log("MovementAction: " + movementAction.ReadValue<Vector2>().normalized);


    //    //Debug.Log("Touch started " + touchControls.Touch.PlayerAttack.ReadValue<Vector2>());

    //}


}
