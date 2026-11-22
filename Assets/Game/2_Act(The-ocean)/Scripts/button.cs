using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class button : MonoBehaviour
{
    [SerializeField] private givekey gk;
    [SerializeField] private GameObject ipanel;
    [SerializeField] private TextMeshProUGUI text;
    public string playertag = "Player";
    public string message1 = "press E to press the button";
    public string message2 = "button needs fixing";

    public bool openable;
    private bool playernear;

    void Update()
    {
        if(playernear && Keyboard.current.eKey.isPressed)
        {
            gk.delete_obstacle();
            gameObject.SetActive(false);
            playernear = false;
            ipanel.SetActive(false);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == playertag)
        {
            ipanel.SetActive(true);
            if (openable)
            {
                playernear = true;
                text.text = message1;
            }
            else
            {
                text.text = message2;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == playertag)
        {
            playernear = false;
            ipanel.SetActive(false);
        }
    }
}

