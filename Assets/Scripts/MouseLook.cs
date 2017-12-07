using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public static MouseLook Instance;
    private ThirdPersonController controller;

    private float x = 0.0f;
    private float y = 0.0f;

    private int mouseXSpeedMod = 3;
    private int mouseYSpeedMod = 2;

    private float MaxViewDistance = 15f;
    private float MinViewDistance = 1f;
    private float desireDistance;
    
    private Transform CameraTarget;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CameraTarget = transform.parent;
        controller = ThirdPersonController.Instance;
        Vector3 Angles = transform.eulerAngles;
        x = Angles.x;
        y = Angles.y;
        desireDistance = 9f;
    }

    void LateUpdate()
    {
        x += Input.GetAxis("Mouse X") * mouseXSpeedMod;
        y += Input.GetAxis("Mouse Y") * mouseYSpeedMod;
        y = ClampAngle(y, -15, 45);
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        desireDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 15f * Mathf.Abs(desireDistance);
        desireDistance = Mathf.Clamp(desireDistance, MinViewDistance, MaxViewDistance);
        Vector3 position = CameraTarget.position - (rotation * Vector3.forward * desireDistance);

        position = CameraTarget.position - (rotation * Vector3.forward * desireDistance + new Vector3(0, -3.0f, 0));

        transform.rotation = rotation;
        transform.position = position;

        if(controller.GetSpeed() > 0.0f)
            CameraTarget.rotation = Quaternion.Lerp(CameraTarget.rotation, Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f), Time.time * 0.08f);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}