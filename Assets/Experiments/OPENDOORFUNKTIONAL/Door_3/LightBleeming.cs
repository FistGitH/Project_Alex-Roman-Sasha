using UnityEngine;
using System.Collections;

public class LightBleeming : MonoBehaviour
{
    public GameObject lamp; // сюда кидаешь объект лампы в Inspector

    public float minDelay = 2f;
    public float maxDelay = 6f;

    private void Start()
    {
        StartCoroutine(BrokenLamp());
    }

    private IEnumerator BrokenLamp()
    {
        while (true)
        {
            // Лампа работает
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Поломка: мерцание
            int flashes = Random.Range(5, 10);

            for (int i = 0; i < flashes; i++)
            {
                lamp.SetActive(!lamp.activeSelf);

                yield return new WaitForSeconds(
                    Random.Range(0.02f, 0.12f)
                );
            }

            // Финальная вспышка
            lamp.SetActive(true);
            yield return new WaitForSeconds(0.05f);

            // Полностью выключилась
            lamp.SetActive(false);
        }
    }
}