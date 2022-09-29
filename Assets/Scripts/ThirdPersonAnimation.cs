using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonAnimation : MonoBehaviour
{
    [SerializeField]
    private ThirdPersonController ThirdPersonController;
    [SerializeField]
    private Animator modelAnimator;    

    private void FixedUpdate()
    {
        modelAnimator.SetFloat("speed", ThirdPersonController.horizontalSpeed/ ThirdPersonController.GetMaxSpeed());
    }

}
