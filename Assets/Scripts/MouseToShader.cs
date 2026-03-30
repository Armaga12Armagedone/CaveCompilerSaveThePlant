using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class MouseToUI : MonoBehaviour
{
    private Material mat;
    private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;

    void Start()
    {
        mat = GetComponent<Graphic>().material;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out Vector2 localPoint))
        {
                        float x = (localPoint.x - rectTransform.rect.x) / rectTransform.rect.width;
            float y = (localPoint.y - rectTransform.rect.y) / rectTransform.rect.height;
            mat.SetVector("_Mouse", new Vector4(x, y, 0, 0));
            float panelAspect = rectTransform.rect.width / rectTransform.rect.height;
            mat.SetFloat("_Aspect", panelAspect);
        }
    }
}
