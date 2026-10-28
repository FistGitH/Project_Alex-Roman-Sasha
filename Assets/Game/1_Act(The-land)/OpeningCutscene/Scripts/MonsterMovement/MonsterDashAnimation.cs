using System;
using System.Collections;
using UnityEngine;

public class MonsterDashAnimation : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private bool faceTarget = true;

    [Header("Wind-Up")]
    [SerializeField, Min(0.01f)] private float windUpDuration = 0.5f;
    [SerializeField, Min(0f)] private float windUpDistance = 0.35f;
    [SerializeField] private float windUpPitchAngle = -5f;
    [SerializeField, Min(0f)] private float headSwayAngle = 4f;
    [SerializeField, Min(0f)] private float headSwayCycles = 1.5f;

    [Header("Dash")]
    [SerializeField, Min(0.01f)] private float dashDuration = 0.15f;
    [SerializeField, Min(0f)] private float dashArcHeight = 0.08f;
    [SerializeField] private float dashPitchAngle = 6f;

    private Coroutine dashRoutine;

    private void Awake()
    {
        if (targetTransform == null)
        {
            throw new InvalidOperationException("MonsterDashAnimation requires a target transform.");
        }
    }

    public void ActivateMonsterDash()
    {
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
        }

        dashRoutine = StartCoroutine(PlayMonsterDash());
    }

    private IEnumerator PlayMonsterDash()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 targetPosition = targetTransform.position;
        Vector3 dashDirection = GetDashDirection(startPosition, targetPosition);
        Vector3 windUpPosition = startPosition - dashDirection * windUpDistance;
        Quaternion targetRotation = faceTarget
            ? Quaternion.LookRotation(dashDirection, Vector3.up)
            : startRotation;

        float elapsedTime = 0f;

        while (elapsedTime < windUpDuration)
        {
            float normalizedTime = elapsedTime / windUpDuration;
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            float swayEnvelope = Mathf.Sin(normalizedTime * Mathf.PI);
            float headSway = Mathf.Sin(normalizedTime * Mathf.PI * 2f * headSwayCycles) * headSwayAngle * swayEnvelope;
            float pitch = windUpPitchAngle * Mathf.Sin(normalizedTime * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPosition, windUpPosition, smoothTime);
            transform.rotation = startRotation * Quaternion.Euler(pitch, headSway, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Quaternion windUpRotation = startRotation * Quaternion.Euler(windUpPitchAngle, 0f, 0f);
        transform.position = windUpPosition;
        transform.rotation = windUpRotation;

        elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            float normalizedTime = elapsedTime / dashDuration;
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            float arcOffset = Mathf.Sin(normalizedTime * Mathf.PI) * dashArcHeight;
            float pitch = Mathf.Sin(normalizedTime * Mathf.PI) * dashPitchAngle;

            transform.position = Vector3.Lerp(windUpPosition, targetPosition, smoothTime) + Vector3.up * arcOffset;
            transform.rotation = Quaternion.Slerp(windUpRotation, targetRotation, smoothTime) * Quaternion.Euler(pitch, 0f, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        dashRoutine = null;
    }

    private Vector3 GetDashDirection(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - startPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            return direction.normalized;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.0001f)
        {
            return forward.normalized;
        }

        return Vector3.forward;
    }
}

