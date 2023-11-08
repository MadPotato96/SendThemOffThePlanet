using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grappling : MonoBehaviour
{
    [Header("Reference")]
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask Grappleable;
    public LineRenderer lr;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCD;
    private float grapplingCDTimer;

    private bool grappling;

    void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (grapplingCDTimer > 0)
            grapplingCDTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (grappling)
        {
            lr.SetPosition(0, gunTip.position);
        }
    }

    private void StartGrapple()
    {
        if (grapplingCDTimer > 0) return;

        grappling = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, Grappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }
    private void ExecuteGrapple()
    {

    }
    private void StopGrapple()
    {
        grappling = false;

        grapplingCDTimer = grapplingCD;

        lr.enabled = false;
    }
       
    // ACTION INPUT
    public void OnGrappling(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            StartGrapple();
        }
    }
}
