using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform _rectTransform;
    public float extraPadding = 0.02f;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        anchorMin.x += extraPadding;
        anchorMin.y += extraPadding;
        anchorMax.x -= extraPadding;
        anchorMax.y -= extraPadding;


        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
    }
}
