using UnityEngine;

public class TurnAround : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Space rotationSpace = Space.Self;

    private float angle = 0f;

    private void Update()
    {
        angle = (angle + rotationSpeed * Time.deltaTime) % 360f;

        if (rotationSpace == Space.Self)
        {
            transform.localRotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);
        }
    }

    public void SetSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public void AddSpeed(float amount)
    {
        rotationSpeed += amount;
    }

    public void StopRotation()
    {
        rotationSpeed = 0f;
    }

    public void StartRotation(float speed)
    {
        rotationSpeed = speed;
    }

    public float GetAngle()
    {
        return angle;
    }

    public void ResetRotation()
    {
        angle = 0f;

        if (rotationSpace == Space.Self)
            transform.localRotation = Quaternion.identity;
        else
            transform.rotation = Quaternion.identity;
    }
}