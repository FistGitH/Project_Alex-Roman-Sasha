using UnityEngine;
using UnityEngine.InputSystem;

public class item : MonoBehaviour
{
    [SerializeField] private InputActionReference interact;
    public inventory theinv;
    public string pltag;

    [Header ("Inventory")]
    public itemdata data;
    public int amount;

    private bool iteractable = false;

    private void Update()
    {
        if(iteractable && interact.action.IsPressed())
        {
            pickup();
        }
    }

    public void pickup()
    {
        theinv.it_add(gameObject.GetComponent<item>());
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == pltag)
        {
            iteractable = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == pltag)
        {
            iteractable = false;
        }
    }
}
