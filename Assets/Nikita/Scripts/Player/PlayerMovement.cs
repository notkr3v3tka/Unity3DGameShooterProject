using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerStats playerStats;

    private CharacterController controller;
    private Transform orientation;

    [Header("Movement")]
    [SerializeField] private float acceleration = 20f;

    [Header("Jump")]
    [SerializeField] private float gravity = -25f;

    [Header("Dash")]
    [SerializeField] private float dashDuration = 0.2f;

    [Header("Knockback")]
    [SerializeField] private float externalForceDamping = 8f;

    [Header("Raycast")]
    [SerializeField] private float rayStartPointY;
    [SerializeField] private float rayLength;
    [SerializeField] private LayerMask groundLayerMask;

    private Vector3 inputDirection;
    private Vector3 dashDirection;
    private Vector3 moveVelocity;
    private Vector3 verticalVelocity;
    private Vector3 dashVelocity;
    private Vector3 externalVelocity;

    private int jumpsUsed;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float groundCheckCooldownTimer;
    private bool isGrounded;

    public float DashTimer => dashTimer;
    public float DashCooldownTimer => dashCooldownTimer;
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        controller = GetComponent<CharacterController>();
        orientation = GetComponent<Transform>();
    }
    private void Update()
    {
        if (UpgradeCardManager.Instance != null && UpgradeCardManager.Instance.IsCardSelectionOpen)
            return;
        ReadInput();
        GroundCheck();
        HandleJump();
        HandleDash();
        HandleHorizontalMovement();
        HandleGravity();
        ApplyExternalForces();
        MoveCharacter();
    }

    private void ReadInput()
    {

    }
    private void GroundCheck()
    {
        Vector3 origin = orientation.position + new Vector3(0, rayStartPointY, 0);
        Vector3 dir = Vector3.down;

        if (Physics.Raycast(origin, dir, rayLength, groundLayerMask))
        {
            if (!isGrounded)
            {
                groundCheckCooldownTimer += Time.deltaTime;
            }

            if (groundCheckCooldownTimer >= 0.2f)
            {
                isGrounded = true;
                groundCheckCooldownTimer = 0f;
            }
        }
        else
        {
            isGrounded = false;
            groundCheckCooldownTimer = 0f;
        }

        if (isGrounded)
        {
            Debug.DrawRay(origin, dir * rayLength, Color.green);
            jumpsUsed = 0;
        }
        else
        {
            Debug.DrawRay(origin, dir * rayLength, Color.red);
        }
    }
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(jumpsUsed < playerStats.MaxJumps)
            {
                verticalVelocity.y = playerStats.JumpHeight;
                jumpsUsed++;
            }
        }
    }
    private void HandleDash()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            if (inputDirection != Vector3.zero)
            {
                dashDirection = inputDirection.normalized;
            }
            else
            {
                dashDirection = orientation.forward.normalized;
            }

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = playerStats.DashCooldown;
        }

        if (isDashing)
        {
            dashVelocity = dashDirection * playerStats.DashSpeed;
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashVelocity = Vector3.zero;
            }
        }
    }
    private void HandleHorizontalMovement()
    {
        float inputX = 0;
        float inputY = 0;
        if (Input.GetKey(KeyCode.W))
        {
            if(inputY < 1)
            {
                inputY += 1;
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (inputY > -1)
            {
                inputY -= 1;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (inputX < 1)
            {
                inputX += 1;
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (inputX > -1)
            {
                inputX -= 1;
            }
        }


        inputDirection = orientation.forward * inputY + orientation.right * inputX;

        inputDirection.Normalize();

        moveVelocity = inputDirection * playerStats.MoveSpeed;
    }
    private void HandleGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }
    private void ApplyExternalForces()
    {
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, externalForceDamping * Time.deltaTime);
    }
    private void MoveCharacter()
    {
        Vector3 finalVelocity;

        finalVelocity = moveVelocity + dashVelocity + externalVelocity;

        finalVelocity.y = verticalVelocity.y;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    public void AddForce(Vector3 force)
    {
        externalVelocity += force;
    }
}
