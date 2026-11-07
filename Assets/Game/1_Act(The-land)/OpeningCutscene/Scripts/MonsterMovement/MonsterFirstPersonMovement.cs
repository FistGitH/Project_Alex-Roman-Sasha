using System;
using UnityEngine;

public class MonsterFirstPersonMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Forward Movement")]
    [SerializeField, Min(0f)] private float movementSpeed = 1.35f;
    [SerializeField, Min(0.01f)] private float deceleration = 1.4f;

    [Header("Heavy Step Rhythm")]
    [SerializeField, Min(0.1f)] private float stepDuration = 0.85f;
    [SerializeField, Range(0f, 0.15f)] private float stepVariation = 0.08f;
    [SerializeField, Min(0f)] private float horizontalSway = 0.075f;
    [SerializeField, Min(0f)] private float verticalLift = 0.035f;
    [SerializeField, Min(0f)] private float footfallDrop = 0.06f;

    [Header("Camera Tilt")]
    [SerializeField, Min(0f)] private float rollAngle = 1.75f;
    [SerializeField, Min(0f)] private float pitchAngle = 0.75f;
    [SerializeField, Min(0.01f)] private float cameraResponseSpeed = 10f;

    private Vector3 initialCameraLocalPosition;
    private Quaternion initialCameraLocalRotation;
    private float currentSpeed;
    private float stepElapsed;
    private float currentStepDuration;
    private float currentStepAmplitude;
    private int currentStepSide = 1;
    private bool isMoving;

    private void Awake()
    {
        if (cameraTransform == null)
        {
            throw new InvalidOperationException("MonsterFirstPersonMovement requires a camera transform.");
        }

        initialCameraLocalPosition = cameraTransform.localPosition;
        initialCameraLocalRotation = cameraTransform.localRotation;
        PrepareNextStep();
    }

    private void Update()
    {
        if (isMoving)
        {
            currentSpeed = movementSpeed;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }

        if (currentSpeed > 0f)
        {
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }

        float movementBlend = movementSpeed > 0f ? currentSpeed / movementSpeed : 0f;

        if (movementBlend > 0.001f)
        {
            AdvanceStep(Time.deltaTime * movementBlend);
        }

        ApplyCameraMotion(movementBlend);
    }

    public void StartMonsterMovement()
    {
        isMoving = true;
    }

    public void StopMonsterMovement()
    {
        isMoving = false;
    }

    private void AdvanceStep(float deltaTime)
    {
        stepElapsed += deltaTime;

        while (stepElapsed >= currentStepDuration)
        {
            stepElapsed -= currentStepDuration;
            currentStepSide *= -1;
            PrepareNextStep();
        }
    }

    private void ApplyCameraMotion(float movementBlend)
    {
        float stepProgress = currentStepDuration > 0f ? stepElapsed / currentStepDuration : 0f;
        float smoothStepProgress = Mathf.SmoothStep(0f, 1f, stepProgress);
        float lateralStep = Mathf.Lerp(-currentStepSide, currentStepSide, smoothStepProgress);
        float stepLift = Mathf.Sin(stepProgress * Mathf.PI);
        float footfall = Mathf.Pow(Mathf.Abs(Mathf.Cos(stepProgress * Mathf.PI)), 3f);
        float amplitude = currentStepAmplitude * movementBlend;

        Vector3 localOffset = new Vector3(
            lateralStep * horizontalSway,
            stepLift * verticalLift - footfall * footfallDrop,
            0f) * amplitude;

        Quaternion localTilt = Quaternion.Euler(
            -footfall * pitchAngle * amplitude,
            0f,
            -lateralStep * rollAngle * amplitude);

        Vector3 targetPosition = initialCameraLocalPosition + localOffset;
        Quaternion targetRotation = initialCameraLocalRotation * localTilt;
        float response = 1f - Mathf.Exp(-cameraResponseSpeed * Time.deltaTime);

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetPosition, response);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRotation, response);
    }

    private void PrepareNextStep()
    {
        currentStepDuration = stepDuration * GetVariationMultiplier();
        currentStepAmplitude = GetVariationMultiplier();
    }

    private float GetVariationMultiplier()
    {
        return UnityEngine.Random.Range(1f - stepVariation, 1f + stepVariation);
    }
}


