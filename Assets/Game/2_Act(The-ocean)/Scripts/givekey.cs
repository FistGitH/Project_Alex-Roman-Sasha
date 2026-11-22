using UnityEngine;

public class givekey : MonoBehaviour
{
    [SerializeField] private GameObject obstacletodelete;
    [SerializeField] private GameObject key;
    [SerializeField] private Vector3 movekey;

    public void delete_obstacle()
    {
        key.transform.position += movekey;
        Destroy(obstacletodelete);
    }
}
