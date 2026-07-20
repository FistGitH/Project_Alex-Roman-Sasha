using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    public Vector3 normalScale = Vector3.one;
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    public Vector3 pressedScale = new Vector3(0.95f, 0.95f, 0.95f);

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = hoverScale;

        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = pressedScale;

        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = hoverScale;
    }
}