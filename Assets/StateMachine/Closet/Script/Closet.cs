using TMPro;
using UnityEngine;
using System.Collections;

public class Closet : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI helptext;
    [SerializeField] private GameObject col;
    [SerializeField] private Animator animator;
    
    private GameObject player;

    private bool IsHidden;


    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && !IsHidden)
        {
            player = other.gameObject;

            helptext.text = "Click E to hide.";

            if (Input.GetKeyDown(KeyCode.E)) 
            { 
                player.transform.position = this.transform.position;
                HideAnimation(); 
            }
        }
        else if (other.tag == "Player" && IsHidden)
        {
            helptext.text = "Click E to exit.";

            if (Input.GetKeyDown(KeyCode.E)) 
            {
                OutAnimation(); 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            helptext.text = "";
        }
    }

    private void Update()
    {

    }

    public void HideAnimation()
    {
        
    }

    public void OutAnimation()
    {

    }

    public void EndOfHideAnimation()
    {
        IsHidden = true;
    }

    public void EndOfOutAnimation()
    {
        col.SetActive(true);
        IsHidden = false;
    }

}