using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public Image health;
    public Image stamina;
    float healthTargetFill = 1f;
    float staminaTargetFill = 1f;

    void Update()
    {
        if (health != null) {
            health.fillAmount = Mathf.Lerp(
                health.fillAmount,
                healthTargetFill,
                Time.deltaTime * 10f
            );
        }

        if (stamina != null) {
            stamina.fillAmount = Mathf.Lerp(
                stamina.fillAmount,
                staminaTargetFill,
                Time.deltaTime * 10f
            );
        }
    }

    public void SetHealth(float curr, float max) {
        Debug.Log($"SetHealth called: {curr}/{max}");
        healthTargetFill = Mathf.Clamp01(curr / max);
    }

    public void SetStamina(float curr, float max)
    {
        staminaTargetFill = Mathf.Clamp01(curr / max);
    }

}
