using UnityEngine;

public class changespawnpoint : MonoBehaviour
{
    [SerializeField] private Vector3 newspawn;
    [SerializeField] private GameObject spawnpoint;
    public string neededtag = "Car";
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == neededtag)
        {
            spawnpoint.transform.position = newspawn;
        }
    }
}
