using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacleProperty : MonoBehaviour
{
    private Animator Animator;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Animator.SetFloat("Offset", (float)Random.Range(0, 11)/10);
    }
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.gameObject.tag == "Player")
    //    {
    //        collision.gameObject.GetComponent<ThirdPersonController>().HitPlayer(collision.GetContact(0).normal * -1f);
    //    }
    //}
}
