using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    CharacterController characterController

    public float walkspeed = 6.0f;
    public float runSpeed = 10.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 move = Vector3.zero;

    void Start ()
    {
        characterController = GetComponent<CharacterController>();

    }
    
    void Update () 
    {
    
    if (characterController.isGrounded) 
        {
            move = new Vector3 (Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        if(Input.GetKey(KeyCode.LeftShift))
            move = transform.TransformDirection(move) * runSpeedpeed;
        else
            move = transform.TransformDirection(move) * walkspeed;
        
        if(Input.GetKey(KeyCode.Space))
            move.y = jumpSpeed;
        }
        
        move.y -= gravity * Time.deltaTime;

    }
}
