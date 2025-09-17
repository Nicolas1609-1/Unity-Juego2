using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(GroundedCheck2D))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 10f;
    public float speed;
    public bool moving = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private bool isSprinting = false;
    private PlayerInput playerInput;
    private GroundedCheck2D groundedCheck;
    private SpriteRenderer spriteRenderer;

    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        groundedCheck = GetComponent<GroundedCheck2D>();
        playerInput = GetComponent<PlayerInput>();
        speed = walkSpeed;

        if (playerInput != null)
        {
            playerInput.actions["Sprint"].started += OnSprintStarted;
            playerInput.actions["Sprint"].canceled += OnSprintCanceled;
        }
    }

    void OnAttack(InputValue inputValue)
    {
        if (!isAttacking) 
        {
            isAttacking = true;
            animator.SetBool("Attack", true);
            rb.linearVelocity = Vector2.zero; 
        }
    }

    
    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("Attack", false);
    }

    void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }

    void OnJump(InputValue inputValue)
    {
        if (!groundedCheck.IsGrounded() || isAttacking) return; 
        rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
    }

    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        isSprinting = true;
        Debug.Log("Sprint started");
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
        Debug.Log("Sprint canceled");
    }

    private void Update()
    {
        // 👇 Si está atacando, bloquear movimiento
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
            return;
        }

        moving = moveInput != Vector2.zero;

        if (moving)
        {
            if (moveInput.x < 0) spriteRenderer.flipX = false;
            else if (moveInput.x > 0) spriteRenderer.flipX = true;

            if (isSprinting)
            {
                speed = runSpeed;
                animator.SetBool("Running", true);
                animator.SetBool("Walking", false);
            }
            else
            {
                speed = walkSpeed;
                animator.SetBool("Running", false);
                animator.SetBool("Walking", true);
            }
        }
        else
        {
            speed = 0f;
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
        }

        if (groundedCheck.IsGrounded())
        {
            animator.SetBool("Jumping", false);
            animator.SetBool("Falling", false);
        }
        else
        {
            if (rb.linearVelocityY < 0)
            {
                animator.SetBool("Falling", true);
                animator.SetBool("Jumping", false);
            }
            else if (rb.linearVelocityY > 0)
            {
                animator.SetBool("Jumping", true);
                animator.SetBool("Falling", false);
            }
        }

        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocityY);
    }
}
