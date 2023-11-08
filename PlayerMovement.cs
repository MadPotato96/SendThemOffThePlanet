using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float _moveSpeed;
    public float _walkSpeed;

    public float _sprintSpeed;

    public float _dashSpeed;
    public float _dashSpeedChangeFactor;

    public float _groundDrag;
    public float _specialDrag;

    public float _jumpForce;
    public float _SlopeJumpForce;
    public float _jumpCooldown;
    public float _airMultiplier;
    bool sprint;
    bool ReadyToJump;
    bool CanDoubleJump;

    [Header("GroundCheck")]
    public Transform planet;
    public float gravMultipler;
    public float _playerHeight;
    public float _playerSkin;
    public float _groundSkin;
    public float _slopeSkin;
    public LayerMask groundMask;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    bool exitingSlope;

    [Header("Camera Blend")]
    public CinemachineFreeLook basicLook;
    public CinemachineFreeLook aimLook;
    public CinemachineBrain cineBrain;
    public Animator animator;

    [Header("Orientation")]
    public Transform orientation;
    public Vector2 _MoveVal;
    Vector3 moveDirection;

    Rigidbody rb;

    private bool OnSpecial;
    public bool isAiming;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        dashing,
        Special,
        air
    }

    public CameraState camState;
    public enum CameraState
    {
        basic,
        aim
    }

    public bool dashing;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        ReadyToJump = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + _playerSkin, groundMask);

        if (!grounded)
        {
            rb.AddForce(new Vector3(0, Physics.gravity.y * gravMultipler, 0), ForceMode.Acceleration);
        }
        
        // handle drag
        if (state == MovementState.walking)
        {
            Debug.Log("Grounded");
            rb.drag = _groundDrag;
        }
        else
        {
            rb.drag = 0;
        }



        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {
        // Mode - Special
        if (OnSpecial)
        {
            state = MovementState.Special;

            rb.freezeRotation = OnSpecial;
            rb.velocity = Vector3.zero;
        }

        // Mode - Dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = _dashSpeed;
            speedChangeFactor = _dashSpeedChangeFactor;
        }

        // Mode - Sprinting
        else if (grounded && sprint)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = _sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = _walkSpeed;
        }

        // Mode - Air
        else if (!grounded)
        {
            state = MovementState.air;

            if (desiredMoveSpeed < _sprintSpeed)
            {
                desiredMoveSpeed = _walkSpeed;
            }
            else
            {
                desiredMoveSpeed = _sprintSpeed;
            }
        }

        


        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        if (lastState == MovementState.dashing)
        {
            keepMomentum = true;
        }

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                _moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movement speed to desire value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - _moveSpeed);
        float startValue = _moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            _moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        _moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void MovePlayer()
    {
        if (state == MovementState.dashing) return;

        // calculate move direction
        moveDirection = orientation.forward * _MoveVal.y + orientation.right * _MoveVal.x;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(getSlopMoveDirection() * _moveSpeed * 10f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                //rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // in ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * _moveSpeed * 10f, ForceMode.Force);
        }

        // on air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * _moveSpeed * 10f * _airMultiplier, ForceMode.Force);
        }

        // turn off gravity while on slope
        if (!OnSpecial)
        {
            rb.useGravity = !OnSlope();
        }
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > _moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * _moveSpeed;
            }
        }

        // limiting speed on ground
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            // limit velocity
            if (flatVel.magnitude > _moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * _moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    /// 
    /// Slope
    /// 
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, _playerHeight * 0.5f + _slopeSkin))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    /// 
    /// CAMERA BLEND
    /// 
    private void SwitchCameraStyle(CameraState newState)
    {
        if (newState == CameraState.basic) animator.Play("BasicCam");
        if (newState == CameraState.aim) animator.Play("AimCam");

        camState = newState;
    }

    private Vector3 getSlopMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public Vector3 CalculateJumpVelocity(Vector3 StartPoint, Vector3 EndPoint, float TrajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = EndPoint.y - StartPoint.y;
        Vector3 displacementXZ = new Vector3(EndPoint.x - StartPoint.x, 0f, EndPoint.z - StartPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * TrajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * TrajectoryHeight / gravity) + Mathf.Sqrt(2 *(displacementY-TrajectoryHeight)/gravity));

        return velocityXZ + velocityY;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        rb.velocity = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
    }

    // INPUT
    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("Move");
        _MoveVal = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (ReadyToJump && grounded)
            {

                ReadyToJump = false;

                CanDoubleJump = true;

                exitingSlope = true;
                // reset y velocity everytime before jump
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                rb.AddForce(transform.up * _jumpForce * 1.2f, ForceMode.Impulse);

                Invoke(nameof(resetJump), _jumpCooldown);
            }
            if (!grounded && CanDoubleJump)
            {
                CanDoubleJump = false;

                // reset y velocity
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
            }
        }
    }
    private void resetJump()
    {
        ReadyToJump = true;

        exitingSlope = false;
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        sprint = context.performed;
    }

    public void OnSpecialMovesHold(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("E button Hold");

            aimLook.m_XAxis.Value = basicLook.m_XAxis.Value;
            aimLook.m_YAxis.Value = basicLook.m_YAxis.Value;
            SwitchCameraStyle(CameraState.aim);
            OnSpecial = true;
            isAiming = true;
        }
    }
    public void OnSpecialMovesRelease(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            Debug.Log("E button Release");

            basicLook.m_XAxis.Value = aimLook.m_XAxis.Value;
            basicLook.m_YAxis.Value = aimLook.m_YAxis.Value;
            SwitchCameraStyle(CameraState.basic);
            OnSpecial = false;
            isAiming = false;
        }
    }
}
