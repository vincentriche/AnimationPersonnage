/// <summary>
/// 
/// </summary>

using UnityEngine;
using System;

[RequireComponent(typeof(Animator))]
public class FootInverseKinematic : MonoBehaviour
{
    [SerializeField]
    protected Animator animator;
    [SerializeField]
    private CapsuleCollider m_collider;
    [SerializeField]
    private bool isKinematic = false;
    [SerializeField]
    private float smooth = 10;

    [SerializeField]
    private Transform footL;
    [SerializeField]
    private Vector3 footLoffset;
    [SerializeField]
    private float weightFootL;

    [SerializeField]
    private Transform footR;
    [SerializeField]
    private Vector3 footRoffset;
    [SerializeField]
    private float weightFootR;

    private Vector3 footPosL;
    private Vector3 footPosR;

    private float transformWeigth = 1.0f;

    private Vector3 defCenter;
    private float defHeight;
    public LayerMask rayLayer;

    void Start()
    {
        defCenter = m_collider.center;
        defHeight = m_collider.height;
    }

    void Update()
    {
        if (isKinematic)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle"))
                IdleUpdateCollider();
        }
        else
        {
            m_collider.center = new Vector3(0, Mathf.Lerp(m_collider.center.y, defCenter.y, Time.deltaTime * smooth), 0);
            m_collider.height = Mathf.Lerp(m_collider.height, defHeight, Time.deltaTime * smooth);
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (isKinematic)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle") && m_collider.attachedRigidbody.velocity.magnitude < 0.1f)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, transformWeigth);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, transformWeigth);

                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, transformWeigth);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, transformWeigth);

                IdleIK();
            }
        }
    }

    void IdleIK()
    {
        RaycastHit hit;
        footPosL = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        if (Physics.Raycast(footPosL + Vector3.up, Vector3.down, out hit, 2.0f, rayLayer))
        {
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + footLoffset);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(Vector3.Exclude(hit.normal, footL.forward), hit.normal));
            footPosL = hit.point;
        }

        footPosR = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        if (Physics.Raycast(footPosR + Vector3.up, Vector3.down, out hit, 2.0f, rayLayer))
        {
            animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + footRoffset);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(Vector3.Exclude(hit.normal, footR.forward), hit.normal));
            footPosR = hit.point;
        }
    }

    void IdleUpdateCollider()
    {
        float dif;
        dif = footPosL.y - footPosR.y;

        if (dif < 0)
            dif *= -1;

        m_collider.center = new Vector3(0, Mathf.Lerp(m_collider.center.y, defCenter.y + dif, Time.deltaTime), 0);
        m_collider.height = Mathf.Lerp(m_collider.height, defHeight - (dif / 2), Time.deltaTime);
    }
}