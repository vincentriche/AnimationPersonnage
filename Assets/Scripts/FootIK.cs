/// <summary>
/// 
/// </summary>

using UnityEngine;
using System;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
    [SerializeField]
    protected Animator animator;
    [SerializeField]
    private CapsuleCollider m_collider;
    [SerializeField]
    private bool isKinematic = false;

    [SerializeField]
    private Transform footL;
    [SerializeField]
    private Vector3 footLoffset;

    [SerializeField]
    private Transform footR;
    [SerializeField]
    private Vector3 footRoffset;

    [SerializeField]
    private GameObject lookObject;

    private Vector3 footPosL;
    private Vector3 footPosR;

    private float transformWeigth = 1.0f;

    public LayerMask rayLayer;

    void Start()
    {
    }

    void Update()
    {
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (isKinematic)
        {
            if (m_collider.attachedRigidbody.velocity.magnitude < 0.1f)
            {

                RaycastHit hit;
                footPosL = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
                if (Physics.Raycast(footPosL + Vector3.up, Vector3.down, out hit, 2.0f, rayLayer))
                {
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + footLoffset);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation);

                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, transformWeigth);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, transformWeigth);
                    footPosL = hit.point;
                }

                footPosR = animator.GetIKPosition(AvatarIKGoal.RightFoot);
                if (Physics.Raycast(footPosR + Vector3.up, Vector3.down, out hit, 2.0f, rayLayer))
                {
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + footRoffset);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation);

                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, transformWeigth);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, transformWeigth);
                    footPosR = hit.point;
                }
            }

            if(lookObject != null)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(lookObject.transform.position);
            }
        }
    }
}
