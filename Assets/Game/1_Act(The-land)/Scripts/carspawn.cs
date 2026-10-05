using UnityEngine;

public class carspawn : MonoBehaviour
{
    [SerializeField] private Car car;

    private Rigidbody carRb;

    private void Start()
    {
        if (car == null)
            return;

        carRb = car.GetComponent<Rigidbody>();

        car.onbreakrules += Spawn;
    }

    private void OnDestroy()
    {
        if (car != null)
        {
            car.onbreakrules -= Spawn;
        }
    }

    public void Spawn()
    {
        if (car == null)
            return;

        // Перемещаем машину
        car.transform.position = transform.position;
        car.transform.rotation = transform.rotation;

        // Сбрасываем скорость
        if (carRb != null)
        {
            carRb.linearVelocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;
        }
    }
}