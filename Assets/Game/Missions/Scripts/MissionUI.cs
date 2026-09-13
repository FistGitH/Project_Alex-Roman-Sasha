using System.Collections;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text completedText;

    [Header("Animation")]
    [SerializeField] private RectTransform uiObject;

    [SerializeField] private float moveDistance = 500f;
    [SerializeField] private float moveDuration = 1f;


    private void OnEnable()
    {
        if (MissionManager.Instance == null)
            return;


        MissionManager.Instance.OnMissionChanged += UpdateMission;
        MissionManager.Instance.OnProgressChanged += UpdateProgress;
        MissionManager.Instance.OnMissionCompleted += MissionCompleted;
    }



    private void OnDisable()
    {
        if (MissionManager.Instance == null)
            return;


        MissionManager.Instance.OnMissionChanged -= UpdateMission;
        MissionManager.Instance.OnProgressChanged -= UpdateProgress;
        MissionManager.Instance.OnMissionCompleted -= MissionCompleted;
    }



    private void Start()
    {
        MissionAnimation();

        if (completedText != null)
            completedText.gameObject.SetActive(false);


        if (MissionManager.Instance != null &&
            MissionManager.Instance.HasMission())
        {
            UpdateMission(
                MissionManager.Instance.GetCurrentMission()
            );


            UpdateProgress(
                MissionManager.Instance.GetCurrentAmount(),
                MissionManager.Instance.GetRequiredAmount()
            );
        }
    }





    // ===============================
    // Œ¡ÕŒ¬À≈Õ»≈ Ã»——»»
    // ===============================


    private void UpdateMission(MissionData mission)
    {
        MissionAnimation();

        if (mission == null)
            return;


        titleText.text = mission.title;

        descriptionText.text = mission.description;



        if (completedText != null)
            completedText.gameObject.SetActive(false);
    }





    // ===============================
    // œ–Œ√–≈——
    // ===============================


    private void UpdateProgress(int current, int required)
    {
        MissionAnimation();

        if (progressText == null)
            return;


        progressText.text =
            current + " / " + required;
    }





    // ===============================
    // «¿¬≈–ÿ≈Õ»≈
    // ===============================


    private void MissionCompleted(MissionData mission)
    {
        MissionAnimation();

        if (completedText == null)
            return;



        completedText.text =
            "Mission Completed!";


        completedText.gameObject.SetActive(true);
    }

    private void MissionAnimation()
    {
        StartCoroutine(MissionAnimationCoroutine());
    }
    private IEnumerator MissionAnimationCoroutine()
    {
        Vector2 startPosition = uiObject.anchoredPosition;

        // Õ‡ÒÍÓÎ¸ÍÓ ‚Ô‡‚Ó
        Vector2 rightPosition = startPosition + new Vector2(moveDistance, 0f);

        // ¬œ–¿¬Œ
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / moveDuration;

            // Smooth easing
            progress = Mathf.SmoothStep(0f, 1f, progress);

            uiObject.anchoredPosition = Vector2.LerpUnclamped(
                startPosition,
                rightPosition,
                progress
            );

            yield return null;
        }

        // ‘ËÍÒËÛÂÏ ÔÓÁËˆË˛ ÒÔ‡‚‡
        uiObject.anchoredPosition = rightPosition;

        // œ‡ÛÁ‡
        yield return new WaitForSeconds(2f);

        // ¬À≈¬Œ
        elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / moveDuration;

            // Smooth easing
            progress = Mathf.SmoothStep(0f, 1f, progress);

            uiObject.anchoredPosition = Vector2.LerpUnclamped(
                rightPosition,
                startPosition,
                progress
            );

            yield return null;
        }

        // ¬ÓÁ‚‡˘‡ÂÏ ÚÓ˜ÌÓ Ì‡ ÏÂÒÚÓ
        uiObject.anchoredPosition = startPosition;
    }



}