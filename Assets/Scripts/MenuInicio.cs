using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    public GameObject fondo;   // panel grande (cierra al tocar afuera)
    public GameObject panel;   // panel chico (con fade)
    public float duracion = 0.6f;

    CanvasGroup grupo;
    Coroutine fade;

    void Awake()
    {
        grupo = panel.GetComponent<CanvasGroup>();
    }

    public void Comenzar()
    {
        SceneManager.LoadScene(1);
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void AbrirPanel()
    {
        fondo.SetActive(true);
        Iniciar(0f, 1f, false);
    }

    public void CerrarPanel()
    {
        Iniciar(1f, 0f, true);
    }

    void Iniciar(float desde, float hasta, bool ocultar)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Fade(desde, hasta, ocultar));
    }

    IEnumerator Fade(float desde, float hasta, bool ocultar)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            grupo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        grupo.alpha = hasta;
        if (ocultar) fondo.SetActive(false);
    }
}