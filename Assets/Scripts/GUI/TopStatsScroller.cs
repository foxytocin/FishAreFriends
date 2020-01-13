using System.Collections;
using UnityEngine;

public class TopStatsScroller : MonoBehaviour
{

    float offScreenOffset = 50f;
    float target;
    float position;

    public float speed = 0.5f;
    private RectTransform[] rectTransform;
    Coroutine fadeInTopStatsCore = null;
    Coroutine fadeOutTopStatsCore = null;

    void Awake() {
        rectTransform = GetComponents<RectTransform>();
        rectTransform[0].anchoredPosition = new Vector2(0, offScreenOffset);
    }


    public void FadeInTopStats() {

        target = 0;
        position = rectTransform[0].anchoredPosition.y;
        fadeInTopStatsCore = StartCoroutine(FadeInTopStatsCore());
    }
    
    private IEnumerator FadeInTopStatsCore() {

        if(fadeOutTopStatsCore != null)
            StopCoroutine(fadeOutTopStatsCore);

        while (position > target)
        {
            position = Mathf.Lerp(position, target, Time.deltaTime * speed);
            rectTransform[0].anchoredPosition = new Vector2(0, position);
            yield return new WaitForEndOfFrame();
        }
    }


    public void FadeOutTopStats() {
        position = rectTransform[0].anchoredPosition.y;
        target = offScreenOffset;
        fadeOutTopStatsCore = StartCoroutine(FadeOutTopStatsCore());
    }


    private IEnumerator FadeOutTopStatsCore() {

        if(fadeInTopStatsCore != null)
                    StopCoroutine(fadeInTopStatsCore);

        while (position < target)
        {
            position = Mathf.Lerp(position, target, Time.deltaTime * speed);
            rectTransform[0].anchoredPosition = new Vector2(0, position);
            yield return new WaitForEndOfFrame();
        }
    }

}
