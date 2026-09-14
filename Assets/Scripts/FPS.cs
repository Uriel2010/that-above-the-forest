using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    CharacterController characterController;

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

    [Header("Deteccion del Monstruo")]
    public targetMonstruo targetMonstruoRef;
    public float distanciaRaycast = 100f;

    // Guardamos a qué monstruo le estamos "fijando" el target,
    // para poder liberarlo cuando dejemos de mirarlo.
    private Monstruo monstruoActual;

    void Start ()
    {
        characterController = GetComponent<CharacterController>();
    }
    
    void Update () 
    {
    
    h_mouse = mouseHorizontal * Input.GetAxis("Mouse X");
    v_mouse += mouseVertical * Input.GetAxis("Mouse Y");
   
    v_mouse = Mathf.Clamp(v_mouse, minRotation, maxRotation);
    cam.transform.localEulerAngles = new Vector3(-v_mouse, 0, 0);

    if (characterController.isGrounded) 
        {
            move = new Vector3 (Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        if(Input.GetKey(KeyCode.LeftShift))
            move = transform.TransformDirection(move) * runSpeed;
        else
            move = transform.TransformDirection(move) * walkspeed;
        
        if(Input.GetKey(KeyCode.Space))
            move.y = jumpSpeed;
        }
        
        move.y -= gravity * Time.deltaTime;

        characterController.Move(move * Time.deltaTime);

        DetectarMonstruo();
    }

    void DetectarMonstruo()
    {
        RaycastHit hit;
        bool golpeoMonstruo = false;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distanciaRaycast))
        {
            Monstruo monstruo = hit.collider.GetComponentInParent<Monstruo>();

            if (monstruo != null)
            {
                golpeoMonstruo = true;

                if (targetMonstruoRef != null)
                {
                    targetMonstruoRef.FijarPosicion(monstruo.transform.position);
                }

                monstruoActual = monstruo;
            }
        }

        // Si dejamos de mirar al monstruo, liberamos el target
        // para que vuelva a seguir al jugador.
        if (!golpeoMonstruo && monstruoActual != null)
        {
            if (targetMonstruoRef != null)
            {
                targetMonstruoRef.LiberarPosicion();
            }

            monstruoActual = null;
        }
    }
}