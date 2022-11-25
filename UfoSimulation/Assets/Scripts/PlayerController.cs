using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region Fields

    private Rigidbody rb;
    private PlayerInputActions InputActions;
    [SerializeField] private Transform attractorRayPos;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private LayerMask cowLayer;
    [SerializeField][Range(1, 5)] private int rayBoxScale;
    [SerializeField][Range(0.1f, 0.9f)] private float smoothValue;
    private bool cowCaptured = false;
    private Cow currentCow = null;
    [SerializeField] private BoxCollider movementBoundsBoxCollider;
    private Bounds limitMovementBounds;
    private Vector2 input;
    private Vector2 smoothInput;
    private Vector3 currentVelocity;
    private bool attract = false;
    [SerializeField] private bool debugGizmos;
    [SerializeField][Range(5, 20)] private int bounceForce = 10; //force applied when player reaches area limits

    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        HandleInput();
    }

    private void OnEnable()
    {
        InputActions.Enable();
    }

    private void OnDisable()
    {
        InputActions.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        limitMovementBounds = movementBoundsBoxCollider.bounds;
    }

    void Update()
    {
        HandleInputMovement();
        Attract();
        RotatePlayerModel();
    }

    private void FixedUpdate()
    {
        LimitPlayerMovement();
        Movement();
        //LimitPlayerMovement();
    }
    #endregion


    #region Gameplay
    void Attract()
    {
        if (!attract)
        {
            if (currentCow != null && cowCaptured)
            {
                currentCow.StopAttraction();
                cowCaptured = false;
            }
            return;
        }
        if (cowCaptured)
            return;
        RaycastHit hit;
        if (Physics.BoxCast(attractorRayPos.position, attractorRayPos.lossyScale * rayBoxScale, -transform.up, out hit, Quaternion.identity, 10f, cowLayer))
        {
            Debug.Log($"Cow found! Cow name: {hit.collider.gameObject.name}");
            //Trigger cow
            currentCow = hit.collider.gameObject.GetComponent<Cow>();
            currentCow.TriggerAttraction(attractorRayPos);
            cowCaptured = true;
        }

    }

    private void RotatePlayerModel()
    {
        playerModel.transform.Rotate(transform.up, 50f * Time.deltaTime);
        //if model has rigidBody use rigidbody of model and move this on FixedUpdate
        //modelRB.MoveRotation(modelRB.rotation * Quaternion.Euler(new Vector3(0f, 50f, 0f) * Time.fixedDeltaTime));
    }

    void Movement()
    {
        rb.MovePosition(rb.position + new Vector3(smoothInput.x, 0f, smoothInput.y) * 10f * Time.fixedDeltaTime);
    }

    void LimitPlayerMovement()
    {
        //limit x,y,z position of rigidbody
        if (rb.position.x < limitMovementBounds.min.x)
        {
            Debug.Log("Limiting x position of player");
            rb.velocity = new Vector3(0f, rb.velocity.y, rb.velocity.z);
            rb.position = new Vector3(limitMovementBounds.min.x, rb.position.y, rb.position.z);
            rb.AddForce(new Vector3(-smoothInput.x, 0f, 0f) * bounceForce, ForceMode.Impulse);
            smoothInput = Vector2.zero;
        }
        if (rb.position.x > limitMovementBounds.max.x)
        {
            Debug.Log("Limiting x position of player");
            rb.velocity = new Vector3(0f, rb.velocity.y, rb.velocity.z);
            rb.position = new Vector3(limitMovementBounds.max.x, rb.position.y, rb.position.z);
            rb.AddForce(new Vector3(-smoothInput.x, 0f, 0f) * bounceForce, ForceMode.Impulse);
            smoothInput = Vector2.zero;
        }

        //there is no movement in y axis
        //if (rb.position.y < limitMovementBounds.min.y)
        //{
        //    Debug.Log("Limiting y position of player");
        //    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //    rb.position = new Vector3(rb.position.x, limitMovementBounds.min.y, rb.position.z);
        //}
        //if (rb.position.y > limitMovementBounds.max.y)
        //{
        //    Debug.Log("Limiting y position of player");
        //    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //    rb.position = new Vector3(rb.position.x, limitMovementBounds.max.y, rb.position.z);
        //}

        if (rb.position.z < limitMovementBounds.min.z)
        {
            Debug.Log("Limiting z position of player");
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0f);
            rb.position = new Vector3(rb.position.x, rb.position.y, limitMovementBounds.min.z);
            rb.AddForce(new Vector3(0f, 0f, -smoothInput.y) * bounceForce, ForceMode.Impulse);
            smoothInput = Vector2.zero;
        }
        if (rb.position.z > limitMovementBounds.max.z)
        {
            Debug.Log("Limiting z position of player");
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0f);
            rb.position = new Vector3(rb.position.x, rb.position.y, limitMovementBounds.max.z);
            rb.AddForce(new Vector3(0f, 0f, -smoothInput.y) * bounceForce, ForceMode.Impulse);
            smoothInput = Vector2.zero;
        }
    }

    #endregion


    #region Input
    private void HandleInput()
    {
        InputActions = new PlayerInputActions();

        InputActions.Player.Movement.performed += ctx =>
        {
            input = ctx.ReadValue<Vector2>();
        };
        InputActions.Player.Movement.canceled += ctx =>
        {
            input = Vector2.zero;
        };

        InputActions.Player.Attract.performed += ctx =>
        {
            attract = !attract;
        };

    }
    void HandleInputMovement()
    {
        input.Normalize();
        smoothInput = Vector3.SmoothDamp(smoothInput, input, ref currentVelocity, smoothValue);
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        if (!debugGizmos)
            return;

        RaycastHit hit;
        if (Physics.BoxCast(attractorRayPos.position, attractorRayPos.lossyScale, -transform.up, out hit, Quaternion.identity, 10f, cowLayer))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attractorRayPos.position + (-attractorRayPos.transform.up * hit.distance), attractorRayPos.lossyScale * rayBoxScale);
        }
    }
    #endregion

}
