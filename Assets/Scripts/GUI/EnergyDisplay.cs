using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyDisplay : MonoBehaviour
{

    GuiOverlay guiOverlay;
    private float fullPosX;
    private float emptyPosX = 70f;
    private float posX;
    private float targetX;
    public float energyStatus = 1000f;
    //private float colorPercent = 100;
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
        posX = fullPosX;
        emptyFishOriginalColor = empty.color;
        warningThreshold = Mathf.FloorToInt((float)Boid.thresholdStarving * 1.2f); // 20 percent more
    }

    void LateUpdate()
    {
        targetX = Map.map(energyStatus, 1000f, 0f, fullPosX, emptyPosX);

        if (posX != targetX)
        {
            posX = Mathf.Lerp(rectTransformOverlay[0].anchoredPosition.x, targetX, 0.3f * Time.deltaTime);
            rectTransformOverlay[0].anchoredPosition = new Vector2(posX, 0);
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
            yield return new WaitForSeconds(1.2f);
            empty.color = new Color32(255, 0, 0, 150);

            yield return new WaitForSeconds(0.5f);
            empty.color = emptyFishOriginalColor;
        }
    }


    public void SetEnergyStatus(int energie)
    {
        energyStatus = (float)energie;
    }
}
