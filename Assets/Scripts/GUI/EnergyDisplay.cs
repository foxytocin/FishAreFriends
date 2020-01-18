using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyDisplay : MonoBehaviour
{

    GuiOverlay guiOverlay;
    private float fullPosX;
    private float emptyPosX = 89f;
    private float fullPosY;
    private float emptyPosY;
    private float posX;
    private float posY;
    private float targetX;
    private float targetY;
    public float energyStatus = 1000f;
    private float colorPercent = 100;
    RectTransform[] rectTransformOverlay;
    Color32 emptyFishOriginalColor;
    public GameObject overlay;
    public Image empty;
    private Coroutine warningPulse;
    private bool warningPulseBlinking = false;
    private int warningThreshold;
    private bool firstStart = true;


    void Awake()
    {
        guiOverlay = FindObjectOfType<GuiOverlay>();
        rectTransformOverlay = overlay.GetComponents<RectTransform>();
        fullPosX = rectTransformOverlay[0].anchoredPosition.x;
        fullPosY = rectTransformOverlay[0].anchoredPosition.y;
        posX = fullPosX;
        posY = fullPosY;
        emptyPosY = -fullPosY;
        emptyFishOriginalColor = empty.color;
        warningThreshold = Boid.thresholdStarving;
    }

    void LateUpdate()
    {
        targetX = Map.map(energyStatus, 1000f, 0f, fullPosX, emptyPosX);
        targetY = Map.map(energyStatus, 1000f, 0f, fullPosY, emptyPosY);
        //colorPercent = Map.map(energyStatus, 1000f, 0f, 100f, 0f);

        if (posX != targetX)
        {
            posX = Mathf.Lerp(rectTransformOverlay[0].anchoredPosition.x, targetX, 0.2f);
            rectTransformOverlay[0].anchoredPosition = new Vector2(posX, posY);
        }

        if (posY != targetY)
        {
            posY = Mathf.Lerp(rectTransformOverlay[0].anchoredPosition.y, targetY, 0.2f);
            rectTransformOverlay[0].anchoredPosition = new Vector2(posX, posY);
        }

        if (energyStatus < warningThreshold && !warningPulseBlinking)
        {
            warningPulseBlinking = true;
            warningPulse = StartCoroutine(WarningPuls());

            if (!firstStart)
                guiOverlay.DisplayMainMessage("Du musst dringend Nahrung finden!", 3, GuiOverlay.MessageType.warning);

            firstStart = false;
        }
        else if (energyStatus >= warningThreshold && warningPulseBlinking)
        {
            warningPulseBlinking = false;
            StopCoroutine(warningPulse);
            empty.color = emptyFishOriginalColor;
        }
    }

    public IEnumerator WarningPuls()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.4f);
            empty.color = new Color32(255, 0, 0, 150);

            yield return new WaitForSeconds(0.2f);
            empty.color = emptyFishOriginalColor;
        }
    }


    public void SetEnergyStatus(int energie)
    {
        energyStatus = (float)energie;
    }
}
