using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ButtonHoverGrowFromLeft : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float widthIncrease = 20f;       // How much to increase width
    public float animationSpeed = 5f;       // How fast the animation happens
    public AudioClip hoverSound;            // Sound to play on hover
    public AudioSource audioSource;         // AudioSource to play the sound

    private RectTransform rectTransform;
    private float originalWidth;
    private float targetWidth;
    private bool isHovered = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalWidth = rectTransform.sizeDelta.x;
        targetWidth = originalWidth;

        // DO NOT CHANGE PIVOT HERE — it causes UI shifts!
        // Instead, set pivot to (0, 0.5) manually in the Editor
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetWidth = originalWidth + widthIncrease;

        if (!isHovered && hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }

        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetWidth = originalWidth;
        isHovered = false;
    }

    void Update()
    {
        float currentWidth = rectTransform.sizeDelta.x;
        float newWidth = Mathf.Lerp(currentWidth, targetWidth, Time.deltaTime * animationSpeed);
        rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
    }
}
