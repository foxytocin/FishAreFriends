using System.Collections;
using UnityEngine;

public class GameEndMenu : MonoBehaviour
{

    float offScreenOffset = 200f;
    float target;
    float position;


    public float speed = 0.5f;
    private RectTransform[] rectTransform;
    Coroutine fadeInTopStatsCore = null;
    Coroutine fadeOutTopStatsCore = null;


    void Awake()
    {
        rectTransform = GetComponents<RectTransform>();
        rectTransform[0].anchoredPosition = new Vector2(13, offScreenOffset);
        position = offScreenOffset;
    }

    public void FadeIn()
    {
        target = -250;
        position = rectTransform[0].anchoredPosition.y;
        fadeInTopStatsCore = StartCoroutine(FadeInTopStatsCore());
    }

    private IEnumerator FadeInTopStatsCore()
    {

        if (fadeOutTopStatsCore != null)
            StopCoroutine(fadeOutTopStatsCore);

        while (position > target)
        {
            position = Mathf.Lerp(position, target, Time.deltaTime * speed);
            rectTransform[0].anchoredPosition = new Vector2(13, position);
            yield return new WaitForEndOfFrame();
        }
    }


    public void FadeOut()
    {
        position = rectTransform[0].anchoredPosition.y;
        target = offScreenOffset;
        fadeOutTopStatsCore = StartCoroutine(FadeOutTopStatsCore());
    }


    private IEnumerator FadeOutTopStatsCore()
    {

        if (fadeInTopStatsCore != null)
            StopCoroutine(fadeInTopStatsCore);

        while (position < target)
        {
            position = Mathf.Lerp(position, target, Time.deltaTime * speed);
            rectTransform[0].anchoredPosition = new Vector2(13, position);
            yield return new WaitForEndOfFrame();
        }
    }

}
