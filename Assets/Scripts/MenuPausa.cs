using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausa;
    bool pausado = false;

    void Start()
    {
        panelPausa.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (pausado) Reanudar();
            else Pausar();
        }
    }

         public void Pausar()
    {
        pausado = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

        public void Reanudar()
    {
        pausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void IrAlInicio()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }
}
