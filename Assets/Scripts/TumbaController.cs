using UnityEngine;

public class TumbaController : MonoBehaviour

{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            animator.SetTrigger("Build");
        }
    }
}
