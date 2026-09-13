using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LampOnOff : MonoBehaviour
{
    [SerializeField] private GameObject lightObject;

    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TextMeshProUGUI interactionText;

    [SerializeField] private AudioSource audioSource;

    private bool playerNear = false;

    private void Awake()
    {
        lightObject.SetActive(false);
        interactionPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            lightObject.SetActive(!lightObject.activeSelf);

            audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            interactionPanel.SetActive(true);
            interactionText.text = "Press [E] to turn the lamp on/off";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            interactionPanel.SetActive(false);
        }
    }
}