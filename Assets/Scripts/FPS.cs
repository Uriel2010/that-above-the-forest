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
    public LanternController linterna;
    public float distanciaRaycast = 20f;
    public float anguloCono = 40f;
    public bool mostrarRaysEnEscena = true;

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
        // Si la linterna no está en modo alto, no detectamos al monstruo.
        bool linternaEnModoAlto = linterna != null && linterna.EstaEnModoAlto();

        bool golpeoMonstruo = false;
        Monstruo monstruoDetectado = null;

        if (linternaEnModoAlto)
        {
            foreach (Vector3 direccion in ObtenerDireccionesCono())
            {
                RaycastHit hit;
                bool lePego = Physics.Raycast(cam.transform.position, direccion, out hit, distanciaRaycast);

                if (mostrarRaysEnEscena)
                {
                    Color colorRay = lePego ? Color.red : Color.yellow;
                    Debug.DrawRay(cam.transform.position, direccion * distanciaRaycast, colorRay);
                }

                if (lePego)
                {
                    Monstruo monstruo = hit.collider.GetComponentInParent<Monstruo>();

                    if (monstruo != null)
                    {
                        golpeoMonstruo = true;
                        monstruoDetectado = monstruo;
                        break;
                    }
                }
            }
        }

        if (golpeoMonstruo)
        {
            if (targetMonstruoRef != null)
            {
                targetMonstruoRef.FijarPosicion(monstruoDetectado.transform.position);
            }

            monstruoActual = monstruoDetectado;
        }

        else if (monstruoActual != null)
        {
            if (targetMonstruoRef != null)
            {
                targetMonstruoRef.LiberarPosicion();
            }

            monstruoActual = null;
        }
    }

    Vector3[] ObtenerDireccionesCono()
    {
        Vector3 forward = cam.transform.forward;
        Vector3 up = cam.transform.up;
        Vector3 right = cam.transform.right;

        return new Vector3[]
        {
            forward,                                           // centro
            Quaternion.AngleAxis(anguloCono, up) * forward,     // derecha
            Quaternion.AngleAxis(-anguloCono, up) * forward,    // izquierda
            Quaternion.AngleAxis(anguloCono, right) * forward,  // abajo
            Quaternion.AngleAxis(-anguloCono, right) * forward, // arriba
        };
    }
}