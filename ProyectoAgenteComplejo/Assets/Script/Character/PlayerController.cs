using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Variables de movimiento")]
    [SerializeField] private float playerSpeed;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float gravityModifier = 2.5f;

    [Header("Mouse settings")]
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Ataque")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private BossBlackboard boss;

    private CharacterController controller;
    private InputSystem_Actions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;

    private float lastAttackTime;
    private bool isAttacking;

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

        // Usamos la antigua acción Jump como ataque.
        // Así la barra espaciadora sirve para atacar sin tocar los inputs.
        inputActions.Player.Jump.performed += ctx => TryAttack();
    }

    void PlayerMovement()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * playerSpeed * Time.deltaTime);

        if (velocity.y < 0)
        {
            velocity.y += gravity * gravityModifier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void MouseCamera()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleAnimations()
    {
        bool isGrounded = controller.isGrounded;
        bool isMoving = moveInput.magnitude > 0.1f;
        bool isFalling = velocity.y < -0.1f && !isGrounded;

        if (animator == null) return;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsFalling", isFalling);
    }

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        Attack();
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        Debug.Log("Jugador: ataque");

        if (animator != null)
        {
            isAttacking = true;
            animator.SetBool("IsAttacking", true);

            StartCoroutine(ResetAttack());

        }

        if (boss == null)
        {
            Debug.LogWarning("No hay BossBlackboard asignado en el PlayerController.");
            return;
        }

        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        float distanceToBoss = Vector3.Distance(origin, boss.transform.position);

        if (distanceToBoss <= attackRange)
        {
            boss.ReceiveDamage(attackDamage);
            Debug.Log("Jugador golpea al robot. Daño: " + attackDamage);
        }
        else
        {
            Debug.Log("Jugador ataca, pero está fuera de rango.");
        }
    }
    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.8f);

        isAttacking = false;

        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
    }
}