using System;
using System.Collections;
using UnityEngine;

public class LampCutsceneAnimation : MonoBehaviour
{
    [SerializeField] private Transform swingTransform;
    [SerializeField] private Light lightSource;
    [SerializeField] private Vector3 swingAngle = new Vector3(0f, 0f, 8f);

    private const float PendulumDuration = 4.5f;
    private const float PendulumFrequency = 1.1f;

    private Quaternion initialSwingRotation;
    private Coroutine pendulumRoutine;
    private Coroutine flickerRoutine;
    private bool pendulumIsLooping;
    private bool flickerIsLooping;
    private bool restoreLightEnabled;
    private float restoreLightIntensity;

    private void Awake()
    {
        if (swingTransform == null)
        {
            throw new InvalidOperationException("LampCutsceneAnimation requires a swing transform.");
        }

        if (lightSource == null)
        {
            throw new InvalidOperationException("LampCutsceneAnimation requires a light source.");
        }

        initialSwingRotation = swingTransform.localRotation;
        restoreLightEnabled = lightSource.enabled;
        restoreLightIntensity = lightSource.intensity;
    }

    public void ActivatePendulumAnimation()
    {
        StopPendulumAnimation();
        pendulumIsLooping = true;
        pendulumRoutine = StartCoroutine(PlayPendulumAnimation());
    }

    public void ActivateLightFlickerAnimation()
    {
        StopLightFlickerAnimation();

        restoreLightEnabled = lightSource.enabled;
        restoreLightIntensity = lightSource.intensity;
        flickerIsLooping = true;
        flickerRoutine = StartCoroutine(PlayLightFlickerAnimation());
    }

    public void ActivateAllLampAnimations()
    {
        ActivatePendulumAnimation();
        ActivateLightFlickerAnimation();
    }

    public void StopAllLampAnimations(bool leaveLightOn)
    {
        StopPendulumAnimation();
        StopLightFlickerAnimation();

        lightSource.enabled = leaveLightOn;
        lightSource.intensity = restoreLightIntensity;
    }

    private IEnumerator PlayPendulumAnimation()
    {
        while (pendulumIsLooping)
        {
            float elapsedTime = 0f;

            while (elapsedTime < PendulumDuration)
            {
                float normalizedTime = elapsedTime / PendulumDuration;
                float damping = 1f - Mathf.SmoothStep(0f, 1f, normalizedTime);
                float offset = Mathf.Sin(elapsedTime * Mathf.PI * 2f * PendulumFrequency) * damping;

                swingTransform.localRotation = initialSwingRotation * Quaternion.Euler(swingAngle * offset);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            swingTransform.localRotation = initialSwingRotation;
            yield return null;
        }

        pendulumRoutine = null;
    }

    private IEnumerator PlayLightFlickerAnimation()
    {
        while (flickerIsLooping)
        {
            lightSource.enabled = true;
            lightSource.intensity = restoreLightIntensity;

            int burstCount = UnityEngine.Random.Range(3, 6);

            for (int burstIndex = 0; burstIndex < burstCount; burstIndex++)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 1f));

                int flashCount = UnityEngine.Random.Range(2, 6);

                for (int flashIndex = 0; flashIndex < flashCount; flashIndex++)
                {
                    lightSource.enabled = false;
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.02f, 0.1f));

                    lightSource.enabled = true;
                    lightSource.intensity = restoreLightIntensity * UnityEngine.Random.Range(0.3f, 0.9f);
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.04f, 0.2f));
                }

                if (UnityEngine.Random.value < 0.45f)
                {
                    lightSource.enabled = false;
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.12f, 0.35f));
                    lightSource.enabled = true;
                    lightSource.intensity = restoreLightIntensity * UnityEngine.Random.Range(0.5f, 0.8f);
                }
            }
        }

        RestoreLightState();
        flickerRoutine = null;
    }

    private void StopPendulumAnimation()
    {
        pendulumIsLooping = false;

        if (pendulumRoutine != null)
        {
            StopCoroutine(pendulumRoutine);
            pendulumRoutine = null;
        }

        swingTransform.localRotation = initialSwingRotation;
    }

    private void StopLightFlickerAnimation()
    {
        flickerIsLooping = false;

        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        RestoreLightState();
    }

    private void RestoreLightState()
    {
        lightSource.enabled = restoreLightEnabled;
        lightSource.intensity = restoreLightIntensity;
    }
}
