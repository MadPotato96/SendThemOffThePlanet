using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Reference")]
    public PlayerMovement pm;
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Transform basicCam;
    public Transform AimCam;
    public Rigidbody rb;

    [SerializeField] private InputAction MoveDirection;

    public Vector2 _inputDirection;
    public float rotationSpeed;

    public Transform aimLookAt;



    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Aim
    }

    private void OnEnable()
    {
        MoveDirection.Enable();
    }

    private void OnDisable()
    {
        MoveDirection.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        MoveDirection.performed += RotateCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (pm.isAiming)
            currentStyle = CameraStyle.Aim;
        else
            currentStyle = CameraStyle.Basic;


        // rotate orientation
        Vector3 viewDir = player.position - new Vector3(basicCam.transform.position.x, player.position.y, basicCam.transform.position.z);
        orientation.forward = viewDir.normalized;

        // rotate player obj
        if (currentStyle == CameraStyle.Basic)
        {
            Vector3 inputDir = orientation.forward * _inputDirection.y + orientation.right * _inputDirection.x;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }
        else if (currentStyle == CameraStyle.Aim)
        {
            Vector3 dirToAimLookAt = player.position - new Vector3(AimCam.transform.position.x, aimLookAt.position.y, AimCam.transform.position.z);
            orientation.forward = dirToAimLookAt.normalized;

            playerObj.forward = dirToAimLookAt.normalized;
        }
    }
    // INPUT
    public void RotateCamera(InputAction.CallbackContext context)
    {
        _inputDirection = context.ReadValue<Vector2>();
    }
}
