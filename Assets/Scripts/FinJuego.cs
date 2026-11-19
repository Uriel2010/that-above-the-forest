using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class FinJuego : MonoBehaviour
{
    public GameObject panelGanaste;
    public GameObject panelPerdiste;
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController jugador;
    public GameObject monstruo;
    public float retrasoPanel = 1f;

    bool terminado = false;
    Vector3 posJugadorInicial;
    Vector3 posMonstruoInicial;
    Quaternion rotMonstruoInicial;

        void Start()
    {
        posJugadorInicial = jugador.transform.position;
        posMonstruoInicial = monstruo.transform.position;
        rotMonstruoInicial = monstruo.transform.rotation;
    }

    void Update()
    {
        if (!terminado && jugador != null && jugador.jugadorMuerto)
        {
            terminado = true;
            Morir();
            Invoke(nameof(Perder), retrasoPanel);
        }
    }
        void Morir()
    {
        // restaurar posiciones
        CharacterController cc = jugador.GetComponent<CharacterController>();
        cc.enabled = false;
        jugador.transform.position = posJugadorInicial;
        cc.enabled = true;

        monstruo.transform.position = posMonstruoInicial;
        monstruo.transform.rotation = rotMonstruoInicial;

        // frenar todo
        monstruo.SetActive(false);
        jugador.enabled = false;
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