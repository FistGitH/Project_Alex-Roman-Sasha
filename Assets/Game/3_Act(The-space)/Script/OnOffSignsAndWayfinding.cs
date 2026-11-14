using UnityEngine;
using System;
using System.Collections;

public class OnOffSignsAndWayfinding : MonoBehaviour
{
    [SerializeField] private GameObject Signs;
    private bool IsAlreadyClosed;

    [SerializeField] private GameObject HelpMassege;
    private CanvasGroup HMCanvas;

    private void Awake()
    {
        HMCanvas = HelpMassege.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        StartCoroutine(FadeOutIn());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (IsAlreadyClosed)
            {
                Signs.SetActive(true);
            }
            else if (!IsAlreadyClosed)
            {
                Signs.SetActive(false);
            }
        }
    }

    private IEnumerator FadeOutIn()
    {
        yield return null;

        for (float i = 0; i <= 1; i += 0.1f)
        {
            HMCanvas.alpha = i;
        }

        yield return new WaitForSeconds(2f);

        for(float i = 1; i >= 0; i -= 0.1f)
        {
            HMCanvas.alpha = i;
        }
    }
}
