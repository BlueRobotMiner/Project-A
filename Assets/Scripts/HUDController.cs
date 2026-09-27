using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public PlayerShield playerShield;
    public Slider hullSlider;
    public TMP_Text hullText;
    public Slider shieldSlider;
    public TMP_Text shieldText;
    public TMP_Text speedText;
    public PlayerStardust playerStardust;
    public TMP_Text stardustText;
    public string speedUnit = " km/s";
    public float barSmoothSpeed = 5f;

    void Start()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerShield == null) playerShield = FindObjectOfType<PlayerShield>();
        if (playerStardust == null) playerStardust = FindObjectOfType<PlayerStardust>();
        SetupSlider(hullSlider);
        SetupSlider(shieldSlider);
    }

    void Update()
    {
        if (playerHealth != null)
        {
            UpdateBar(hullSlider, hullText, playerHealth.currentHealth, playerHealth.maxHealth);
        }
        if (playerShield != null)
        {
            UpdateBar(shieldSlider, shieldText, playerShield.currentShield, playerShield.maxShield);
        }
        if (stardustText != null && playerStardust != null)
        {
            stardustText.text = playerStardust.stardust.ToString();
        }
        if (speedText != null && WaveManager.Instance != null)
        {
            speedText.text = Mathf.RoundToInt(WaveManager.Instance.CurrentSpeed) + speedUnit;
        }
    }

    void SetupSlider(Slider slider)
    {
        if (slider == null) return;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    void UpdateBar(Slider slider, TMP_Text label, float current, float max)
    {
        float target = max > 0f ? current / max : 0f;
        if (slider != null)
        {
            slider.value = Mathf.Lerp(slider.value, target, barSmoothSpeed * Time.deltaTime);
        }
        if (label != null)
        {
            label.text = Mathf.CeilToInt(current) + "/" + Mathf.CeilToInt(max);
        }
    }
}
