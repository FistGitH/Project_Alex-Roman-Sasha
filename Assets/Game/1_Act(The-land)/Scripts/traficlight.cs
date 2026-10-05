using UnityEngine;

public class traficlight : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private GameObject rlight;
    [SerializeField] private GameObject ylight;
    [SerializeField] private GameObject glight;

    [Header("Settings")]
    [SerializeField] private float changeTime = 2f;

    private float timer;

    private int lightState = 0;

    // 0 = Green
    // 1 = Yellow
    // 2 = Red
    // 3 = Yellow

    private bool redOn = false;

    private void Awake()
    {
        SetGreen();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeTime)
        {
            timer = 0f;

            lightState++;

            if (lightState > 3)
            {
                lightState = 0;
            }

            ChangeLight();
        }
    }

    private void ChangeLight()
    {
        switch (lightState)
        {
            case 0:
                SetGreen();
                break;

            case 1:
                SetYellow();
                break;

            case 2:
                SetRed();
                break;

            case 3:
                SetYellow();
                break;
        }
    }

    private void SetGreen()
    {
        rlight.SetActive(false);
        ylight.SetActive(false);
        glight.SetActive(true);

        redOn = false;
    }

    private void SetYellow()
    {
        rlight.SetActive(false);
        ylight.SetActive(true);
        glight.SetActive(false);

        redOn = false;
    }

    private void SetRed()
    {
        rlight.SetActive(true);
        ylight.SetActive(false);
        glight.SetActive(false);

        redOn = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!redOn)
            return;

        Car car = other.GetComponentInParent<Car>();

        if (car != null)
        {
            car.breakroadrules();
        }
    }
}