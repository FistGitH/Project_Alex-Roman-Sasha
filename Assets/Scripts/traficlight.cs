using Unity.VisualScripting;
using UnityEngine;

public class traficlight : MonoBehaviour
{
    [SerializeField] private GameObject rlight;
    [SerializeField] private GameObject ylight;
    [SerializeField] private GameObject glight;
    private GameObject lastlight;
    private bool greenlast;
    private bool redon;
    private float time;
    private Car c;
    private void Awake()
    {
        rlight.SetActive(false);
        ylight.SetActive(false);
        glight.SetActive(true);
        lastlight = glight;
        greenlast = true;   
    }
    void Update()
    {
        time += Time.deltaTime;
        if (time > 2)
        {
            time = 0;
            changelight(light());
        }
        if(c != null)
        {
            c.breakroadrules();
            c = null;
        }
    }
    private GameObject light()
    {
        if (lastlight != ylight)
        {
            lastlight = ylight;
            redon = false;
        }
        else if (greenlast)
        {
            lastlight = rlight;
            greenlast = false;
            redon = true;
        }
        else
        {
            lastlight = glight;
            greenlast = true;
        }
        return lastlight;
    }

    private void changelight(GameObject correctlight)
    {
        rlight.SetActive(false);
        ylight.SetActive(false);
        glight.SetActive(false);
        correctlight.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Car>() != null && redon)
        {
            c = other.GetComponent<Car>();
        }
    }
}
