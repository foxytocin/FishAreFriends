using UnityEngine;

public class EnergyDisplay : MonoBehaviour
{

    private float fullPosX;
    private float emptyPosX = 115f;
    private float fullPosY;
    private float emptyPosY = -26f;
    private float posX;
    private float posY;
    private float targetX;
    private float targetY;
    public float energyStatus = 1000f;
    private float colorPercent = 100;
    RectTransform[] rectTransform;
    SpriteRenderer spritRenderer;


    void Awake()
    {
        rectTransform = GetComponents<RectTransform>();
        spritRenderer = GetComponent<SpriteRenderer>();
        fullPosX = rectTransform[0].anchoredPosition.x;
        fullPosY = rectTransform[0].anchoredPosition.y;
        posX = fullPosX;
        posY = fullPosY;
        emptyPosY = -fullPosY;
    }

    void LateUpdate()
    {
        targetX = map(energyStatus, 1000f, 0f, fullPosX, 90f);
        targetY = map(energyStatus, 1000f, 0f, fullPosY, emptyPosY);
        colorPercent = map(energyStatus, 1000f, 0f, 100f, 0f);

        if (posX != targetX)
        {
            posX = Mathf.Lerp(posX, targetX, 0.3f);
            rectTransform[0].anchoredPosition = new Vector2(posX, posY);
            //spritRenderer.color = Color.Lerp(new Color32(137, 59, 65, 255), new Color32(59, 137, 66, 255), colorPercent);
        }

        if (posY != targetY)
        {
            posY = Mathf.Lerp(posY, targetY, 0.3f);
            rectTransform[0].anchoredPosition = new Vector2(posX, posY);
            //spritRenderer.color = Color.Lerp(new Color32(137, 59, 65, 255), new Color32(59, 137, 66, 255), colorPercent);
        }
    }


    public void SetEnergyStatus(int energie)
    {
        energyStatus = (float)energie;
    }


    private static float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
