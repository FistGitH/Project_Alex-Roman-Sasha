using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("UI")]

    public GameObject SettingsPanel;
    public GameObject EveryButton;


    private void Awake()
    {
        SettingsPanel.SetActive(false);
    }
    public void OpenSettings()
    {
        SettingsPanel.SetActive(true);
        EveryButton.SetActive(false);
    }
    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
        EveryButton.SetActive(true);
    }
}