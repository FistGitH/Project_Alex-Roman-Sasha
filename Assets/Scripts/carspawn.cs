using UnityEngine;

public class carspawn : MonoBehaviour
{
    [SerializeField] private Car car;
    void Start()
    {
        car.onbreakrules += spawn;
    }
    public void spawn()
    {
        car.gameObject.transform.position = gameObject.transform.position;
    }
}
