using UnityEngine;
using TMPro;

public class CreditsScroller : MonoBehaviour
{
    public float scrollSpeed = 20f;  // Pixels per second
    public float resetPositionY = 800f; // Starting Y position (below the screen)
    public float endPositionY = 1200f;  // End Y position (above the screen)

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // Start the text below the screen
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = resetPositionY;
        rectTransform.anchoredPosition = pos;
    }

    void Update()
    {
        // Move text upwards
        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // If the text goes above the end position, reset to bottom
        if (rectTransform.anchoredPosition.y >= endPositionY)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = resetPositionY;
            rectTransform.anchoredPosition = pos;
        }
    }
}
