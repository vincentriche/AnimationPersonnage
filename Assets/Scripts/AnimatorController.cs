using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator animator;
    private ThirdPersonController controller;    

    void Start()
    {
        controller = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetFloat("speed", controller.GetSpeed());
        animator.SetFloat("direction", controller.GetDirection());
        animator.SetBool("isJumping", controller.IsJumping());
        animator.SetBool("isCrouching", controller.IsCrouching());
    }
}
