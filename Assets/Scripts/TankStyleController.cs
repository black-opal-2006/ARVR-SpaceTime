using UnityEngine;
using UnityEngine.InputSystem;

public class TankStyleController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float turnSpeed = 120f;
    public float gravity = -15f;
    public float jumpHeight = 1.2f;
    public float speedSmoothTime = 0.1f;
    public Animator animator;

    private CharacterController controller;
    private float verticalVelocity;
    private float currentSpeed;
    private float speedVelocity;

    // other scripts can check this to know if Robert is on the ground
    public bool IsGrounded
    {
        get { return controller.isGrounded; }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float move = 0f;
        float turn = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            move = 1f;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            move = -1f;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            turn = 1f;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            turn = -1f;
        }

        // turn the character left or right
        transform.Rotate(0f, turn * turnSpeed * Time.deltaTime, 0f);

        // move the character forward or backward
        Vector3 direction = transform.forward * move * moveSpeed;

        // simple gravity so the character falls if not grounded
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        direction.y = verticalVelocity;

        controller.Move(direction * Time.deltaTime);

        // reset the jump flag once we land again
        if (controller.isGrounded && verticalVelocity <= -1f)
        {
            if (animator != null)
            {
                animator.SetBool("Jump", false);
            }
        }

        // tell the animator whether we are touching the ground
        if (animator != null)
        {
            animator.SetBool("Grounded", controller.isGrounded);
        }

        // smoothly change the speed value instead of snapping instantly
        float targetSpeed = Mathf.Abs(move) * moveSpeed;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);

        // update animator so the walk/run animation plays smoothly
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
            animator.SetFloat("MotionSpeed", 1f);
        }
    }

    // called from GravityShiftController when there is no wall in front
    public void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (animator != null)
        {
            animator.SetBool("Jump", true);
        }
    }
}