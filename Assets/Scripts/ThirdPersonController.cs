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

    [SerializeField]
    private GameObject spine;
    [SerializeField]
    private GameObject head;
    [SerializeField]
    private GameObject hat;
    private Vector3 hatPosition;
    private Quaternion hatRotation;
    [SerializeField]
    private GameObject glasses;
    private Vector3 glassesPosition;
    private Quaternion glassesRotation;

    [SerializeField]
    public GameObject treePrefab;

    private Vector3 movement;
    private float cap;
    private Vector3 groundSpeed;
    private Vector3 crouchSpeed;

    private float groundDeaccelerationDampX;
    private float groundDeaccelerationDampY;
    private float groundDeaccelerationTime = 0.1f;
    private float walkAndRunTransitionTime = 0.2f;
    private Rigidbody m_rigidbody;

    bool freeze = false;
    bool isRagdolled = false;
    
    private Animator animator;

    private void Awake()
    {
        Instance = this;
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        cap = maxWalkingSpeed;
        hatPosition = hat.transform.localPosition;
        hatRotation = hat.transform.localRotation;
        glassesPosition = glasses.transform.localPosition;
        glassesRotation = glasses.transform.localRotation;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Standing Up"))
            freeze = true;
        else
            freeze = false;

        AnimatorParameters();

        if (Input.GetKey(KeyCode.C) && (State == State.Grounded || State == State.Running))
            State = State.Crouched;
        else if (!Input.GetKey(KeyCode.C) && State == State.Crouched)
            State = State.Grounded;

        if (Input.GetKey(KeyCode.LeftShift) && State == State.Grounded)
            State = State.Running;
        else if (!Input.GetKey(KeyCode.LeftShift) && State == State.Running)
            State = State.Grounded;


        if (Input.GetKeyDown(KeyCode.R) && State == State.Ragdolled)
        {
            State = State.Grounded;
            EnableRagdoll(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && (State == State.Grounded || State == State.Running || State == State.Jumped || State == State.Crouched))
        {
            State = State.Ragdolled;
            EnableRagdoll(true);
        }

        // Spawn Tree
        if (Input.GetKeyDown(KeyCode.F))
            SpawnTree();
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (state == State.Jumped)
            state = State.Grounded;

        if (collision.collider.gameObject.tag != "Terrain")
        {
            State = State.Ragdolled;
            EnableRagdoll(true);
        }
    }

    private void FixedUpdate()
    {
        if (freeze == false)
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

    private void AnimatorParameters()
    {
        animator.SetFloat("speed", GetSpeed());
        animator.SetFloat("direction", GetDirection());
        animator.SetBool("isJumping", IsJumping());
        animator.SetBool("isCrouching", IsCrouching());
    }

    private void EnableRagdoll(bool b)
    {
        GetComponent<FootIK>().enabled = !b;
        GetComponent<CapsuleCollider>().enabled = !b;
        animator.enabled = !b;

        var rigColliders = GetComponentsInChildren<Collider>();
        var rigRigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Collider col in rigColliders)
        {
            if (col.tag == "Player")
                col.enabled = !b;
            else
                col.enabled = b;
        }

        Vector3 velocity = m_rigidbody.velocity;
        foreach (Rigidbody rb in rigRigidbodies)
        {
            if (rb.tag == "Player")
                rb.isKinematic = b;
            else
            {
                rb.isKinematic = !b;
                rb.detectCollisions = b;
                rb.useGravity = b;
                rb.velocity = velocity;
                //rb.AddForce(new Vector3(10.0f, 0.0f, 10.0f));
            }
        }

        if (b == true)
        {
            isRagdolled = true;
            hat.transform.parent = transform;
            glasses.transform.parent = transform;
        }
        else
        {
            Vector3 cameraPosition = GetComponentInChildren<Camera>().transform.position;
            if (isRagdolled == true)
            {
                Vector3 position = new Vector3(spine.transform.position.x, 0.04363012f, spine.transform.position.z);
                transform.position = position;

                GetComponentInChildren<Camera>().transform.position = Vector3.Lerp(GetComponentInChildren<Camera>().transform.position, cameraPosition, 1.0f);

                hat.transform.parent = head.transform;
                hat.transform.localPosition = hatPosition;
                hat.transform.localRotation = hatRotation;

                glasses.transform.parent = head.transform;
                glasses.transform.localPosition = glassesPosition;
                glasses.transform.localRotation = glassesRotation;

                animator.Play("Standing Up");
                isRagdolled = false;
            }
        }
    }

    public bool IsJumping()
    {
        return state == State.Jumped;
    }

    public bool IsCrouching()
    {
        return state == State.Crouched;
    }

    public bool IsRagdolling()
    {
        return isRagdolled;
    }
    
    public void SpawnTree()
    {
        Vector3 cameraForward = GetComponentInChildren<MouseOrbiter>().transform.forward;
        Vector3 p = transform.position + cameraForward * 15.0f + transform.up * 2.0f;
        GameObject o = Instantiate(treePrefab, p, Quaternion.identity);
        o.GetComponent<Rigidbody>().AddForce(-cameraForward * 100000.0f);
    }
}

public enum State
{
    Grounded,
    Running,
    Jumped,
    Crouched,
    Ragdolled
};
