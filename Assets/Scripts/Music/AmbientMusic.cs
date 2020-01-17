using System.Collections;
using UnityEngine;

public class AmbientMusic : MonoBehaviour
{
    private AudioSource audioMusic;
    private AudioEchoFilter audioEcho;
    private float volume = 1f;
    public float speed = 0.8f;
    public float minVolume = 0.2f;

    Coroutine playAndFade = null;
    Coroutine fadeAndPause = null;

    void Awake()
    {
        audioMusic = GetComponent<AudioSource>();
        audioEcho = GetComponent<AudioEchoFilter>();
        audioMusic.volume = volume;
    }


    public void StartAmbientMusic()
    {
        playAndFade = StartCoroutine(PlayAndFade());
    }

    public void StopAmbientMusic()
    {
        fadeAndPause = StartCoroutine(FadeAndPause());
    }


    private IEnumerator PlayAndFade()
    {
        if (fadeAndPause != null)
            StopCoroutine(fadeAndPause);

        audioEcho.enabled = false;
        audioMusic.Play();

        while (volume < 1f)
        {
            volume += Time.deltaTime * speed;
            audioMusic.volume = volume;
            yield return new WaitForEndOfFrame();
        }

        volume = 1f;
        audioMusic.volume = volume;
    }


    private IEnumerator FadeAndPause()
    {
        if (playAndFade != null)
            StopCoroutine(playAndFade);


        while (volume > minVolume)
        {
            if (volume < ((1 + minVolume) / 2f))
                audioEcho.enabled = true;

            volume -= Time.deltaTime * (speed / 3f);
            audioMusic.volume = volume;
            yield return new WaitForEndOfFrame();
        }

        audioMusic.Pause();
        volume = minVolume;
        audioMusic.volume = volume;
    }
}
