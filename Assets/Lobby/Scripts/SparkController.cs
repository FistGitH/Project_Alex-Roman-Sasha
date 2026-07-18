using UnityEngine;
using System.Collections;

public class SparkController : MonoBehaviour
{
    public ParticleSystem sparks;
    public Light sparkLight;
    public AudioSource audioSource;

    public float minDelay = 2f;
    public float maxDelay = 8f;

    void Start()
    {
        StartCoroutine(SparkLoop());
    }

    IEnumerator SparkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            sparks.Play();

            if (audioSource != null)
                audioSource.Play();

            if (sparkLight != null)
                StartCoroutine(FlashLight());
        }
    }

    IEnumerator FlashLight()
    {
        sparkLight.intensity = 6;

        yield return new WaitForSeconds(0.05f);

        sparkLight.intensity = 0;
    }
}