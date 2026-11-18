using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausa;
    


void Start()
{
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}
   public void Pausar()
{
    Debug.Log("Pausar llamado");
    panelPausa.SetActive(true);
    Time.timeScale = 0f;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

    public void Reanudar()
    {
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void IrAlInicio()
    {
        Time.timeScale = 1f; // importante: si no, la otra escena arranca congelada
        SceneManager.LoadScene(0);
    }
    
void Update()
{
    if (Input.GetMouseButtonDown(0) && !panelPausa.activeSelf && !EventSystem.current.IsPointerOverGameObject())
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (panelPausa.activeSelf) Reanudar();
        else Pausar();
    }
}
}