using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThirdPersonAnimation : NetworkBehaviour
{
    [SerializeField]
    private ThirdPersonController ThirdPersonController;
    [SerializeField]
    private Animator modelAnimator;

    public override void OnNetworkSpawn()
    {
        ThirdPersonController.JumpStarted += handleJump;
        ThirdPersonController.DoubleJumpStarted += handleDoubleJump;
    }
    public override void OnNetworkDespawn()
    {
        ThirdPersonController.JumpStarted -= handleJump;
        ThirdPersonController.DoubleJumpStarted -= handleDoubleJump;
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
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
