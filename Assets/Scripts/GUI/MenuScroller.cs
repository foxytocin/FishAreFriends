﻿using System.Collections;
using UnityEngine;

public class MenuScroller : MonoBehaviour
{

    float offScreenOffset = 145f;
    float target;
    float position;

    public float speed = 0.5f;
    private RectTransform[] rectTransform;
    Coroutine fadeInTopStatsCore = null;
    Coroutine fadeOutTopStatsCore = null;

    void Awake() {
        rectTransform = GetComponents<RectTransform>();
        rectTransform[0].anchoredPosition = new Vector2(offScreenOffset, 0);
    }


    public void FadeIn() {
            target = -110;
            position = rectTransform[0].anchoredPosition.x;
            fadeInTopStatsCore = StartCoroutine(FadeInTopStatsCore());
    }
    
    private IEnumerator FadeInTopStatsCore() {

        if(fadeOutTopStatsCore != null)
            StopCoroutine(fadeOutTopStatsCore);

        while (position > target)
        {
            position = Mathf.Lerp(position, target, Time.deltaTime * speed);
            rectTransform[0].anchoredPosition = new Vector2(position, 0);
            yield return new WaitForEndOfFrame();
        }
    }


    public void FadeOut() {
        position = rectTransform[0].anchoredPosition.x;
        target = offScreenOffset;
        fadeOutTopStatsCore = StartCoroutine(FadeOutTopStatsCore());
    }


    private IEnumerator FadeOutTopStatsCore() {

        if(fadeInTopStatsCore != null)
                    StopCoroutine(fadeInTopStatsCore);

        while (position < target)
        {
            position = Mathf.Lerp(position, target, Time.deltaTime * speed);
            rectTransform[0].anchoredPosition = new Vector2(position, 0);
            yield return new WaitForEndOfFrame();
        }
    }

}
