using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ground check and gravity
    [SerializeField] private GameObject planet;
    [SerializeField] private LayerMask planetMask;
    public float gravity = 9.81f;

    float _distanceToGround;
    Vector3 GroundNormal;

    // input
    private InputAction_Player playerActionAsset;
    private InputAction move;

    // movement 
    private Rigidbody rb;
    [SerializeField] private float _speed = 4;
    [SerializeField] private Vector2 _moveVal;
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private Vector3 forceDirection = Vector3.zero;

    [SerializeField] private Transform playerCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        playerActionAsset = new InputAction_Player();
    }
    private void OnEnable()
    {
        playerActionAsset.Movement.Jump.started += OnJump;
        move = playerActionAsset.Movement.Move;
        playerActionAsset.Movement.Enable();
    }

    private void OnDisable()
    {
        playerActionAsset.Movement.Jump.started -= OnJump;
        playerActionAsset.Movement.Enable();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //handleMove();
        Vector3 readInput = new Vector3(_moveVal.x, 0, _moveVal.y) * _speed;

        Vector3 camForward = playerCamera.forward;
        Vector3 camRight = playerCamera.right;

        camForward.y = 0;
        camRight.y = 0;

        Vector3 moveDirection = (readInput.x * camRight) + (readInput.z * camForward);

        if (_moveVal.x != 0 || _moveVal.y != 0)
        {
            rb.MovePosition(new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z));
        }
        else
        {

            // find a way to fix movement and rotation
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        // gravity and rotation
        Vector3 gravDirection = (transform.position - planet.transform.position).normalized;
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, -transform.up, out hit, 10, planetMask))
        {
            GroundNormal = hit.normal;
        }

        if (!isGrounded())
        {
            rb.velocity += gravDirection * -gravity * Time.deltaTime;
        }

        //Vector3 horizontalVelocity = rb.velocity;
        //horizontalVelocity.y = 0;
        //if (horizontalVelocity.sqrMagnitude > _speed * _speed)
        //{
        //    rb.velocity = horizontalVelocity.normalized * _speed + Vector3.up * rb.velocity.y;
        //}

        //

        //Quaternion toRotation = Quaternion.FromToRotation(transform.up, GroundNormal) * transform.rotation;
        //transform.rotation = toRotation;


        Debug.DrawRay(transform.position, -transform.up, Color.red, 1, true);

        // look rotation
    }
    private void FixedUpdate()
    {
        
    }

    private bool isGrounded()
    {
        Ray ray = new Ray(this.transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 1f, planetMask))
        {
            Debug.Log("Grounded");
            rb.velocity = Vector3.zero;
            return true;            
        }
        else
        {
            return false;
        }
    }

    // INPUT
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _moveVal = context.ReadValue<Vector2>();
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded())
        {
            forceDirection = transform.up * _jumpHeight * Time.deltaTime;
        }
    }
}
