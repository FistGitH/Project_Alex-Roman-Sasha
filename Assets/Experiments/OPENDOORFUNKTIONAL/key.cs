using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class key : MonoBehaviour
{
    [SerializeField] private OpenDoor door;
    [SerializeField] private GameObject ipanel;
    [SerializeField] private TextMeshProUGUI text;
    public string playertag;
    public string interactivemessage = "press E to pick up the key";
    private bool playernear;

    void Update()
    {
        if (Keyboard.current.eKey.isPressed && playernear)
        {
            door.GiveKey();
            ipanel.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == playertag)
        {
            playernear = true;
            text.text = interactivemessage;
            ipanel.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == playertag)
        {
            playernear = false;
            ipanel.SetActive(false);
        }
    }
}
