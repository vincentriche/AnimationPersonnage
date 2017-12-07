using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThirdPersonController : MonoBehaviour
{
    public static ThirdPersonController Instance;

    [Header("Mouvements")]
    [SerializeField]
    private float accelerationSpeed = 50000f;
    [SerializeField]
    private float maxWalkingSpeed = 4.0f;
    [SerializeField]
    private float maxRunningSpeed = 8.0f;
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
    private float groundDeaccelerationDampX;
    private float groundDeaccelerationDampY;
    private float groundDeaccelerationTime = 0.05f;
    private Rigidbody m_rigidbody;

    private void Awake()
    {
        Instance = this;
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
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

        if (state == State.Grounded || state == State.Running)
        {
            Move(m);
            CapSpeed();
        }

        if (Input.GetKeyDown(KeyCode.Space)
            && (state == State.Grounded || state == State.Running))
        {
            Jump();
        }
    }

    private void Move(Vector3 move)
    {
        if (move.x != 0f || move.z != 0f)
        {
            m_rigidbody.AddRelativeForce(move * accelerationSpeed * Time.deltaTime);
        }
        // Déaccélération
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
        float cap = (state == State.Grounded)
                    ? maxWalkingSpeed : maxRunningSpeed;
        if (rigidbody_movement.magnitude > cap)
        {
            rigidbody_movement.Normalize();
            rigidbody_movement *= cap;
            m_rigidbody.velocity = new Vector3(rigidbody_movement.x, m_rigidbody.velocity.y, rigidbody_movement.y);
        }
    }
}


public enum State
{
    Grounded,
    Running,
    Jumped,
};
