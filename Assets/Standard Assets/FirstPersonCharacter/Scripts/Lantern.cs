using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    // Colgá este script en el Player (o en el objeto que tenga la luz) y arrastrá
    // la referencia de la Light en el Inspector. Click derecho para prender/apagar.
    public class FlashlightController : MonoBehaviour
    {
        [SerializeField] private Light m_FlashlightLight;
        [SerializeField] private bool m_StartsOn = false;
        [SerializeField] private AudioClip m_ToggleSound; // opcional: sonido de click al prender/apagar

        private AudioSource m_AudioSource;
        private bool m_IsOn;

        private void Start()
        {
            // si no la asignaste a mano, intenta encontrar una Light en los hijos
            if (m_FlashlightLight == null)
            {
                m_FlashlightLight = GetComponentInChildren<Light>();
            }

            m_AudioSource = GetComponent<AudioSource>();

            m_IsOn = m_StartsOn;
            ApplyState();
        }

        private void Update()
        {
            // click derecho = botón 1
            if (Input.GetMouseButtonDown(1))
            {
                Toggle();
            }
        }

        private void Toggle()
        {
            if (m_FlashlightLight == null)
            {
                Debug.LogWarning("FlashlightController: no hay ninguna Light asignada.");
                return;
            }

            m_IsOn = !m_IsOn;
            ApplyState();

            if (m_ToggleSound != null && m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(m_ToggleSound);
            }
        }

        private void ApplyState()
        {
            if (m_FlashlightLight != null)
            {
                m_FlashlightLight.enabled = m_IsOn;
            }
        }
    }
}