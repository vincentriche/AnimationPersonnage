using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThirdPersonController : MonoBehaviour
{
    public static ThirdPersonController Instance;
    private MouseLook MooseLook;

    [Header("Mouvements")]
    [SerializeField]
    private float accelerationSpeed = 50000f;
    [SerializeField]
    private float maxWalkingSpeed = 4.0f;
    [SerializeField]
    private float maxRunningSpeed = 8.0f;
    [SerializeField]
    private float maxCrouchingSpeed = 2.0f;
    [SerializeField]
    private float jumpVelocity = 200f;
    [SerializeField]
    private float maxGroundSlope = 60f;
    public float MaxGroundSlope
    {
        get
        {
            return maxGroundSlope;
        }
    }

    [SerializeField]
    private State state;
    public State State
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
        }
    }
    
    private Vector3 movement;
    private float cap;
    private Vector3 groundSpeed;
    private Vector3 crouchSpeed;
    
    private float groundDeaccelerationDampX;
    private float groundDeaccelerationDampY;
    private float groundDeaccelerationTime = 0.1f;
    private float walkAndRunTransitionTime = 0.2f;
    private Rigidbody m_rigidbody;

    private void Awake()
    {
        Instance = this;
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        MooseLook = MouseLook.Instance;
        cap = maxWalkingSpeed;
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        movement = new Vector3(m_rigidbody.velocity.x, 0f, m_rigidbody.velocity.z);
        Vector3 m = new Vector3(h, 0f, v);

        if (state == State.Grounded || state == State.Running || state == State.Crouched)
        {
            Move(m);
            CapSpeed();
        }

        if (Input.GetKeyDown(KeyCode.Space)
            && (state == State.Grounded || state == State.Running))
        {
            Jump();
        }

        groundSpeed = m_rigidbody.velocity;
        groundSpeed.y = 0;
    }

    private void Move(Vector3 move)
    {
        if (move.x != 0f || move.z != 0f)
        {
            m_rigidbody.AddRelativeForce(move * accelerationSpeed * Time.deltaTime);
        }
        else if (Input.GetAxisRaw("Vertical") == 0f && Input.GetAxisRaw("Horizontal") == 0f)
        {
            movement.x = Mathf.SmoothDamp(movement.x, 0f, ref groundDeaccelerationDampX, groundDeaccelerationTime);
            movement.y = Mathf.SmoothDamp(movement.y, 0f, ref groundDeaccelerationDampY, groundDeaccelerationTime);
            m_rigidbody.velocity = new Vector3(movement.x, m_rigidbody.velocity.y, movement.y);
        }
    }

    private void Jump()
    {
        state = State.Jumped;
        m_rigidbody.AddRelativeForce(0f, jumpVelocity, 0f);
    }

    private void CapSpeed()
    {
        Vector2 rigidbody_movement = new Vector2(m_rigidbody.velocity.x, m_rigidbody.velocity.z);

        if (state == State.Grounded)
            cap = Mathf.SmoothDamp(cap, maxWalkingSpeed, ref groundDeaccelerationDampY, walkAndRunTransitionTime);

        if (state == State.Running)
            cap = Mathf.SmoothDamp(cap, maxRunningSpeed, ref groundDeaccelerationDampY, walkAndRunTransitionTime);

        if (state == State.Crouched)
            cap = Mathf.SmoothDamp(cap, maxCrouchingSpeed, ref groundDeaccelerationDampY, walkAndRunTransitionTime);

        if (rigidbody_movement.magnitude > cap)
        {
            rigidbody_movement.Normalize();
            rigidbody_movement *= cap;
            m_rigidbody.velocity = new Vector3(rigidbody_movement.x, m_rigidbody.velocity.y, rigidbody_movement.y);
        }
    }

    public float GetSpeed()
    {
        return groundSpeed.magnitude;
    }

    public float GetDirection()
    {
        return Input.GetAxis("Horizontal"); // MooseLook.transform.rotation.eulerAngles.x; // Input.GetAxis("Horizontal");
    }
    
    public bool IsJumping()
    {
        return state == State.Jumped;
    }

    public bool IsCrouching()
    {
        return state == State.Crouched;
    }
}

public enum State
{
    Grounded,
    Running,
    Jumped,
    Crouched
};
