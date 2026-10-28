/* using UnityEngine;
using UnityEngineUI;

public class StaminaBar : MonoBehaviour
{
    public Slider staminaBar;
    private int maxStamina = 100;
    private int currentStamina;
    public static StaminaBar instance; 

    private void Awake ()
    {
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.Value = maxStamina;
    }

    public void UseStamina (int amount)
    {
        if (currentStamina - amount >= 0)
        {
         currentStamina -= amount;
         staminaBar.Value = currentStamina;   
        }
        else
        {
            Debug.Log ("No se puede correr");
        }
    }
    }


    
 */