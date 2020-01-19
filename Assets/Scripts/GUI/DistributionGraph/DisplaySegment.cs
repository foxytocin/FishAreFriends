using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class DisplaySegment : MonoBehaviour
{
    private RectTransform[] rectTransform;
    private Image segmentImage;
    private Color32 segmentColor;
    private float targetPosX;


    void Awake()
    {
        rectTransform = GetComponents<RectTransform>();
        segmentImage = GetComponent<Image>();
    }

    public void SetColor(Color32 color_)
    {
        double f = 0.5; // desaturate by 20%
        double L = 0.3 * color_.r + 0.6 * color_.g + 0.1 * color_.b;
        double new_r = color_.r + f * (L - color_.r);
        double new_g = color_.g + f * (L - color_.g);
        double new_b = color_.b + f * (L - color_.b);

        segmentImage.color = new Color32((byte)new_r, (byte)new_g, (byte)new_b, 255);
    }

    public void SetPosition(float posX)
    {
        targetPosX = posX;
    }

    float tmpPosX;
    public void SetInitialPosition(Vector2 pos)
    {
        rectTransform[0].anchoredPosition = pos;
        targetPosX = rectTransform[0].anchoredPosition.x;
        tmpPosX = targetPosX;
        StartCoroutine(UpdateGraph());
    }

    private IEnumerator UpdateGraph()
    {
        while (true)
        {
            if (rectTransform[0].anchoredPosition.x != targetPosX)
            {
                tmpPosX = Mathf.Lerp(rectTransform[0].anchoredPosition.x, targetPosX, 0.3f * Time.deltaTime);
                rectTransform[0].anchoredPosition = new Vector2(tmpPosX, 0);
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
