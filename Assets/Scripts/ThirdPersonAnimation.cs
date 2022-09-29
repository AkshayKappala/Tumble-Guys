using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonAnimation : MonoBehaviour
{
    [SerializeField]
    private ThirdPersonController ThirdPersonController;
    [SerializeField]
    private Animator modelAnimator;

    private void OnEnable()
    {
        ThirdPersonController.JumpStarted += handleJump;
        ThirdPersonController.DoubleJumpStarted += handleDoubleJump;
    }
    private void OnDisable()
    {
        ThirdPersonController.JumpStarted -= handleJump;
        ThirdPersonController.DoubleJumpStarted -= handleDoubleJump;
    }
    private void FixedUpdate()
    {
        modelAnimator.SetFloat("speed", ThirdPersonController.horizontalSpeed/ ThirdPersonController.GetMaxSpeed());
        modelAnimator.SetBool("isGround", ThirdPersonController.isGrounded);
    }
    void handleJump()
    {
        modelAnimator.SetTrigger("jumpGo");
    }
    void handleDoubleJump()
    {
        modelAnimator.SetTrigger("doubleJumpGo");
    }
}
