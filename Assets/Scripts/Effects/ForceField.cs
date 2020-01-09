using UnityEngine;
using System.Collections;

public class ForceField : MonoBehaviour
{

public float pulseSpeed = 0.5f;
public float fadeSpeed = 0.5f;
private Material material;
private AudioSource audioPulse;
private float alpha = 0f;
private float power = 1.5f;

    void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_Alpha", alpha);
        audioPulse = GetComponent<AudioSource>();
        alpha = material.GetFloat("_Alpha");
    }


    public void StartPulse() {
        StartCoroutine(FadeToOneAndPulse());
    }

    public void StopPulse() {
        StartCoroutine(FadeToZeroAndStop());
    }

    public void SetColor(Color color) {
        material.SetColor("_Emission", color);
    }

    private IEnumerator Pulse()
    {
        while(true) {
            power = 1.5f + Mathf.PingPong(Time.time * pulseSpeed, 4.5f);
            material.SetFloat("_Power", power);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FadeToZeroAndStop()
    {
        while (alpha > 0f) {
            alpha -= (fadeSpeed / 2f) * Time.deltaTime;
            audioPulse.volume -= (fadeSpeed / 2f) * Time.deltaTime;
            material.SetFloat("_Alpha", alpha);
            yield return new WaitForEndOfFrame();
        }

        StopCoroutine(Pulse());
        alpha = 0;
        material.SetFloat("_Alpha", alpha);
        audioPulse.Stop();
        yield return null;
    }

    private IEnumerator FadeToOneAndPulse()
    {
        audioPulse.volume = 1f;
        audioPulse.Play();

        while (alpha < 2f) {
            alpha += fadeSpeed * Time.deltaTime;
            material.SetFloat("_Alpha", alpha);
            yield return new WaitForEndOfFrame();
        }

        alpha = 2f;
        StartCoroutine(Pulse());
        material.SetFloat("_Alpha", alpha);
        yield return null;
    }

}
