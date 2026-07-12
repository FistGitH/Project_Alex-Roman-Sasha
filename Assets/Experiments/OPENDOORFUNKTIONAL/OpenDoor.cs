using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class OpenDoor : MonoBehaviour
{
    private Animator animator;

    [Header("Door")]
    [SerializeField] private bool Needkey;
    [SerializeField] private bool Getkey;

    [SerializeField] private GameObject Locker;

    [SerializeField] private float HoldTime = 2f;

    [Header("UI")]
    [SerializeField] private Slider openSlider;
    [SerializeField] private CanvasGroup sliderCanvasGroup;

    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text interactionText;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.red;

    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private float resultTime = 0.3f;

    private Image sliderFill;

    private bool playerInside;
    private float holdTimer;
    private float resultTimer;
    private bool showingResult;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (openSlider != null)
        {
            sliderFill = openSlider.fillRect.GetComponent<Image>();
            openSlider.value = 0;
            openSlider.gameObject.SetActive(false);
        }

        if (sliderCanvasGroup != null)
        {
            sliderCanvasGroup.alpha = 0;
        }

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        // ==============================
        // ÄÂÅÐÜ ÁÅÇ ÊËÞ×À
        // ==============================
        if (!Needkey)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                animator.SetBool("EnterTrigger", true);

                Locker.SetActive(false);

                if (interactionText != null)
                    interactionText.text = "Opened";
            }

            return;
        }

        // ==============================
        // Àíèìàöèÿ ðåçóëüòàòà
        // ==============================
        if (showingResult)
        {
            resultTimer -= Time.deltaTime;

            if (resultTimer <= 0)
            {
                sliderCanvasGroup.alpha = Mathf.MoveTowards(
                    sliderCanvasGroup.alpha,
                    0,
                    fadeSpeed * Time.deltaTime);

                if (sliderCanvasGroup.alpha <= 0)
                {
                    showingResult = false;

                    openSlider.gameObject.SetActive(false);
                    openSlider.value = 0;
                    holdTimer = 0;

                    sliderFill.color = normalColor;

                    if (interactionText != null)
                        interactionText.text = "Hold [E] to Unlock";
                }
            }

            return;
        }

        // ==============================
        // Äâåðü ñ êëþ÷îì
        // ==============================
        if (Keyboard.current.eKey.isPressed)
        {
            openSlider.gameObject.SetActive(true);
            sliderCanvasGroup.alpha = 1;

            sliderFill.color = normalColor;

            holdTimer += Time.deltaTime;
            openSlider.value = holdTimer / HoldTime;

            if (holdTimer >= HoldTime)
            {
                showingResult = true;
                resultTimer = resultTime;

                if (Getkey)
                {
                    sliderFill.color = successColor;
                    animator.SetBool("EnterTrigger", true);

                    Locker.SetActive(false);

                    if (interactionText != null)
                        interactionText.text = "Opened";
                }
                else
                {
                    sliderFill.color = failColor;

                    if (interactionText != null)
                        interactionText.text = "Door is Locked";

                    Debug.Log("Door is closed...");
                }
            }
        }
        else
        {
            holdTimer = 0;

            if (openSlider.gameObject.activeSelf)
            {
                openSlider.value = 0;
                openSlider.gameObject.SetActive(false);
                sliderCanvasGroup.alpha = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);

            if (Needkey)
                interactionText.text = "Hold [E] to Unlock";
            else
                interactionText.text = "Press [E] to Open";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        holdTimer = 0;
        showingResult = false;

        if (!Needkey)
        {
            animator.SetBool("EnterTrigger", false);
            Locker.SetActive(true);
        }

        if (openSlider != null)
        {
            openSlider.value = 0;
            openSlider.gameObject.SetActive(false);
        }

        if (sliderCanvasGroup != null)
        {
            sliderCanvasGroup.alpha = 0;
        }

        if (sliderFill != null)
        {
            sliderFill.color = normalColor;
        }

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }

    public void GiveKey()
    {
        Getkey = true;
    }
}