using UnityEngine;
using UnityEngine.InputSystem; 
public class PlayerController : MonoBehaviour
{
    [Header("Variables de movimiento")] 
    [SerializeField] private float playerSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpModifier = -2f;
    [SerializeField] private float gravityModifier = 2.5f;

    [Header("Mouse settings")]
    [SerializeField] float mouseSensitivity = 100f;
    [SerializeField] Transform cameraPivot; 

    private CharacterController controller;
    private InputSystem_Actions inputActions;

    [Header("Animations")]
    [SerializeField] Animator animator;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;

    
    private void Awake()
    {
        PlayerInputs();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        InitialSetting();
    }

    private void Update()
    {
        PlayerMovement();
        MouseCamera();
        HandleAnimations();
    }

    void InitialSetting()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void PlayerInputs()
    {
        inputActions = new InputSystem_Actions();

        
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => Jump();


    }

    void PlayerMovement()
    {

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            animator.SetBool("IsJumping", false);
        }

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * playerSpeed * Time.deltaTime);

        
        if(velocity.y < 0)
        {
            velocity.y += gravity * gravityModifier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * jumpModifier * gravity);
        }

        animator.SetBool("IsJumping", true);
    }

    void MouseCamera()
    {
        
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    
    void HandleAnimations()
    {
        bool isGrounded = controller.isGrounded;
        bool isMoving = moveInput.magnitude > 0.1f && isGrounded;
        bool isFalling = velocity.y < -0.1f && !isGrounded; 

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsFalling", isFalling);
    }

}
