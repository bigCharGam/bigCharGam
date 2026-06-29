using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : CharacterBase
{
    [Header("PlayerStats")]
    public float maxStamina;
    public float currentStamina;
    public float staminaRegenRate;
    public float staminaRegenDelay;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;

    private void Reset()
    {
        maxHealth = 100f;
        MoveSpeed = 10f;
        baseAttackPower = 10f;
        maxStamina = 100f;
        staminaRegenRate = 5f;
        staminaRegenDelay = 2f;
    }

    protected override void Start()
    {
        base.Start();
        currentStamina = maxStamina;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.y > 0.5f && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    protected override void Update()
    {
        base.Update();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * MoveSpeed, rb.linearVelocity.y);
    }
}
