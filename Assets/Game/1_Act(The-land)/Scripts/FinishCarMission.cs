using System.Collections;
using UnityEngine;

public class FinishCarMission : MonoBehaviour
{
    [Header("Cars")]
    [SerializeField] private GameObject CarForAnim;

    [Header("Cameras")]
    [SerializeField] private GameObject CurrentCamera;
    [SerializeField] private GameObject AnimationCamera;

    [Header("Parking")]
    [SerializeField] private Transform ParkingPoint;

    [SerializeField] private float driveTime = 4f;

    // Чем выше - тем плавнее разгон/торможение
    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private GameObject OurCar;

    private bool missionStarted = false;

    private void Awake()
    {
        if (CarForAnim != null)
        {
            CarForAnim.SetActive(false);
        }

        if (AnimationCamera != null)
        {
            AnimationCamera.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (missionStarted)
            return;

        Car car = other.GetComponentInParent<Car>();

        if (car == null)
            return;

        missionStarted = true;

        OurCar = car.gameObject;

        StartMission();
    }

    private void StartMission()
    {
        // Сохраняем позицию настоящей машины
        Vector3 startPosition =
            OurCar.transform.position;

        Quaternion startRotation =
            OurCar.transform.rotation;

        // Выключаем настоящую машину
        OurCar.SetActive(false);

        // Включаем машину для кат-сцены
        CarForAnim.SetActive(true);

        // Ставим её точно туда,
        // где была настоящая машина
        CarForAnim.transform.position =
            startPosition;

        CarForAnim.transform.rotation =
            startRotation;

        // Переключаем камеры
        if (CurrentCamera != null)
        {
            CurrentCamera.SetActive(false);
        }

        if (AnimationCamera != null)
        {
            AnimationCamera.SetActive(true);
        }

        // Убираем trigger миссии вниз
        transform.position =
            new Vector3(
                transform.position.x,
                -10f,
                transform.position.z
            );

        StartCoroutine(LastAnimation());
    }

    private IEnumerator LastAnimation()
    {
        if (ParkingPoint == null)
        {
            Debug.LogError(
                "ParkingPoint не установлен!"
            );

            yield break;
        }

        Vector3 startPosition =
            CarForAnim.transform.position;

        Quaternion startRotation =
            CarForAnim.transform.rotation;

        Vector3 targetPosition =
            ParkingPoint.position;

        Quaternion targetRotation =
            ParkingPoint.rotation;

        float timer = 0f;

        while (timer < driveTime)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / driveTime
                );

            // Плавный разгон и торможение
            float smoothT =
                movementCurve.Evaluate(t);

            // Плавно двигаем
            CarForAnim.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );

            // Плавно поворачиваем
            CarForAnim.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );

            yield return null;
        }

        // В конце ставим абсолютно точно
        CarForAnim.transform.position =
            targetPosition;

        CarForAnim.transform.rotation =
            targetRotation;

        // Немного ждём после парковки
        yield return new WaitForSeconds(1.5f);

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        Debug.Log("Parking finished!");

        // Тут потом загрузишь следующую сцену
    }
}