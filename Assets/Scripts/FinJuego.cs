using UnityEngine;
using UnityEngine.SceneManagement;

public class FinJuego : MonoBehaviour
{
    public GameObject panelGanaste;
    public GameObject panelPerdiste;
    public MuerteJugador jugador;
    public float retrasoPanel = 1f;

    bool terminado = false;

    void Update()
    {
        if (!terminado && jugador != null && jugador.EstaMuerto())
        {
            terminado = true;
            Invoke(nameof(Perder), retrasoPanel);
        }
    }

    public void Ganar()
    {
        Mostrar(panelGanaste);
    }

    public void Perder()
    {
        Mostrar(panelPerdiste);
    }

    void Mostrar(GameObject panel)
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Salir()
    {
        Application.Quit();
    }
}