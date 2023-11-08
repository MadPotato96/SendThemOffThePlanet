using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dashing : MonoBehaviour
{
    [Header("Reference")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Dashing")]
    public float dashForce;
    private float dashUpwardForce;
    public float dashUpwardForce_Ground;
    public float dashUpwardForce_Air;
    public float dashDuration;

    [Header("Settings")]
    public bool useCameraForward;
    public bool allowAllDirections;
    public bool disableGravity;
    public bool resetVel;

    [Header("Cooldown")]
    public float dashCD;
    private float dashCDTimer;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (dashCDTimer > 0)
        {
            dashCDTimer -= Time.deltaTime;
        }

        if (pm.grounded)
            dashUpwardForce = dashUpwardForce_Ground;
        else
            dashUpwardForce = dashUpwardForce_Air;
    }

    private void Dash()
    {
        if (dashCDTimer > 0)
        {
            return;
        }
        else
        {
            dashCDTimer = dashCD;
        }

        pm.dashing = true;

        Transform forwardT;

        if (useCameraForward)
            forwardT = playerCam;   // where camera facing
        else
            forwardT = orientation;  //where character facing

        Vector3 direction = GetDirection(forwardT);

        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)         // disable gravity during dash
        {
            rb.useGravity = false;
        }

        delayedForceApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceApply;
    private void DelayedDashForce()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.dashing = false;

        if (disableGravity)
        {
            rb.useGravity = true;
        }
    }

    // INPUT ACTION
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Dash();
        }
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        Vector3 direction = new Vector3();

        if (allowAllDirections)
            direction = forwardT.forward * pm._MoveVal.y + forwardT.right * pm._MoveVal.x;
        else
            direction = forwardT.forward;
        if (pm._MoveVal.y == 0 && pm._MoveVal.x == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }
}
