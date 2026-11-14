using UnityEngine;
using UnityEngine.InputSystem;

public class item : MonoBehaviour
{
    [SerializeField] private InputActionReference interact;
    [SerializeField] public inventory theinv;
    public string pltag;

    [Header ("Inventory")]
    [SerializeField] public itemdata data;
    [SerializeField] public int amount;

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
