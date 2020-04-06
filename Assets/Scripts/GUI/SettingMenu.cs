using System.Collections;
using UnityEngine;

public class SettingMenu : MonoBehaviour
{

    float offScreenOffset = 200f;
    float target;
    float position;
    public AudioSource bubbleSound;

    public float speed = 0.5f;
    private RectTransform[] rectTransform;
    Coroutine fadeInTopStatsCore = null;
    Coroutine fadeOutTopStatsCore = null;
    private Tutorial tutorial;

    void Awake()
    {
        rectTransform = GetComponents<RectTransform>();
        rectTransform[0].anchoredPosition = new Vector2(13, offScreenOffset);
        position = offScreenOffset;
        tutorial = FindObjectOfType<Tutorial>();
        bubbleSound = GetComponent<AudioSource>();
    }

    public void PlayBubbleSound() {
        if(!bubbleSound.isPlaying)
        bubbleSound.Play();
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
