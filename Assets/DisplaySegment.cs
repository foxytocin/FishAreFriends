using UnityEngine;
using UnityEngine.UI;

public class DisplaySegment : MonoBehaviour
{
    private RectTransform[] rectTransform;
    private Image segmentImage;
    private Color32 segmentColor;


    void Awake()
    {
        rectTransform = GetComponents<RectTransform>();
        segmentImage = GetComponent<Image>();
    }

    public void SetColor(Color32 color_)
    {
        segmentImage.color = color_;
    }

    public void SetPosition(float posX)
    {
        rectTransform[0].anchoredPosition = new Vector2(posX, 0);
    }

    public void SetInitialPosition(Vector2 pos)
    {
        rectTransform[0].anchoredPosition = pos;
    }
}
