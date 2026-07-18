using UnityEngine;
using TMPro;

public class ResolutionDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;


    public void ChangeResolution()
    {
        switch (dropdown.value)
        {
            case 0:
                SettingsManager.Instance.SetResolution(1920, 1080);
                break;

            case 1:
                SettingsManager.Instance.SetResolution(1600, 900);
                break;

            case 2:
                SettingsManager.Instance.SetResolution(1280, 720);
                break;

            case 3:
                SettingsManager.Instance.SetResolution(1024, 768);
                break;
        }
    }
}