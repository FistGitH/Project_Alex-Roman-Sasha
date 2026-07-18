using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]

    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Slider mouseSlider;


    [Header("Dropdowns")]

    public TMP_Dropdown qualityDropdown;



    private void OnEnable()
    {
        LoadUI();
    }



    public void LoadUI()
    {
        if (SettingsManager.Instance == null)
            return;


        // Volume

        if (volumeSlider != null)
        {
            volumeSlider.value =
                SettingsManager.Instance.volume;
        }



        // Brightness

        if (brightnessSlider != null)
        {
            brightnessSlider.value =
                SettingsManager.Instance.brightness;
        }



        // Mouse

        if (mouseSlider != null)
        {
            mouseSlider.value =
                SettingsManager.Instance.mouseSensitivity;
        }



        // Quality

        if (qualityDropdown != null)
        {
            qualityDropdown.value =
                SettingsManager.Instance.highQuality ? 1 : 0;
        }
    }
}