using UnityEngine;

public class StateManagement : MonoBehaviour
{
    private ThirdPersonController controller;

    void Start()
    {
        controller = ThirdPersonController.Instance;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.C) && (controller.State == State.Grounded || controller.State == State.Running))
            controller.State = State.Crouched;
        else if (!Input.GetKey(KeyCode.C) && controller.State == State.Crouched)
            controller.State = State.Grounded;

        if (Input.GetKey(KeyCode.R) && (controller.State == State.Grounded || controller.State == State.Running || controller.State == State.Jumped || controller.State == State.Crouched))
            controller.State = State.Ragdolled;
        else if (!Input.GetKey(KeyCode.R) && controller.State == State.Ragdolled)
            controller.State = State.Grounded;

        if (Input.GetKey(KeyCode.LeftShift) && controller.State == State.Grounded)
            controller.State = State.Running;
        else if (!Input.GetKey(KeyCode.LeftShift) && controller.State == State.Running)
            controller.State = State.Grounded;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (controller.State == State.Jumped)
            controller.State = State.Grounded;
    }
}
