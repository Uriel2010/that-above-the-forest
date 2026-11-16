using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider staminaBar;
    public static StaminaBar instance;

    public float maxStamina = 100f;
    public float drainRate = 20f;
    public float rechargeRate = 15f;
    public float resumeThreshold = 25f;

    private float currentStamina;
    private bool exhausted;

    public bool CanRun => !exhausted;

    void Awake() { instance = this; }

    void Start()
    {
        currentStamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = maxStamina;
    }
        void Update()
    {
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);

        if (shiftPressed && !exhausted)
        {
            currentStamina -= drainRate * Time.deltaTime;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                exhausted = true;
            }
        }
        else
        {
            currentStamina += rechargeRate * Time.deltaTime;

            if (exhausted && currentStamina >= resumeThreshold)
                exhausted = false;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        staminaBar.value = currentStamina;
    }
}


    
