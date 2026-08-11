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

    public Camera cam;
    public float mouseHorizontal = 3.0f;
    public float mouseVertical = 6.0f;
    public float minRotation = -65.0f;
    public float maxRotation = 60.0f;
    float h_mouse, v_mouse;

    private Vector3 move = Vector3.zero;

    void Start ()
    {
        characterController = GetComponent<CharacterController>();
    }
    
    void Update () 
    {
    
    h_mouse = mouseHorizontal * InputGetAxis("Mouse X");
    v_mouse += mouseVertical * InputGetAxis("Mouse Y");
   
    v_mouse = Math.Clamp(v_mouse, minRotation, maxRotation);
    cam.transform.localEulerAngles = new Vector3(-v_mouse, 0, 0);

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

        characterController.Move(move * Time.deltaTime);
    }
}
