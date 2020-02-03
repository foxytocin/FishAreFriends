using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowStream : MonoBehaviour
{


    public ParticleSystem p_oben;
    public ParticleSystem p_rechts;
    public ParticleSystem p_unten;
    public ParticleSystem p_links;
    private bool running = false;



    public void playFlowStream(Vector3 vec)
    {
        if (p_oben.isPlaying || p_rechts.isPlaying || p_unten.isPlaying || p_links.isPlaying)
            return;

        float x = vec.x;
        float y = vec.y;
        bool horizontal = false;

        if (Mathf.Abs(x) > Mathf.Abs(y))
            horizontal = true;

        if (horizontal)
        {
            if (x > 0)
            {
                p_rechts.Play();
            }
            else
            {
                // starte links
                p_links.Play();
            }
        }
        else
        {
            if (y > 0)
            {
                // starte oben
                p_oben.Play();
            }
            else
            {
                // starte unten
                p_unten.Play();
            }
        }
    }
}