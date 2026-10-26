using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

// Se agrega como componente en el GameObject del jugador (el que tiene FirstPersonController).
public class MuerteJugador : MonoBehaviour
{
    [SerializeField] private bool reiniciarEscena = true;
    [SerializeField] private float retrasoReinicio = 2f;

    private bool muerto = false;

    public bool EstaMuerto() { return muerto; }

    // Llamado por Monstruo cuando toca al jugador.
    public void Morir()
    {
        if (muerto)
            return;

        muerto = true;
        Debug.Log("El jugador murió.");

        // Se bloquea el control del jugador
        FirstPersonController controlador = GetComponent<FirstPersonController>();
        if (controlador != null)
            controlador.enabled = false;

        // Acá se puede agregar pantalla de "Game Over", sonido, animación, etc.

        if (reiniciarEscena)
            Invoke(nameof(Reiniciar), retrasoReinicio);
    }

    void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
