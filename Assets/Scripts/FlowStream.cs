using UnityEngine;
using System.Collections;

public class FlowStream : MonoBehaviour
{
    public Leader leader;
    public ParticleSystem p_oben;
    public ParticleSystem p_rechts;
    public ParticleSystem p_unten;
    public ParticleSystem p_links;
    private bool running = false;


    public void playFlowStream(Vector3 vec, Vector3 pos)
    {
        if (running)
            return;

        running = true;
        StartCoroutine(runTimer());
        float x = vec.x;
        float y = vec.y;
        bool horizontal = false;

        if (Mathf.Abs(x) > Mathf.Abs(y))
            horizontal = true;

        if (horizontal)
        {
            if (x > 0)
            {
                ParticleSystem ps = Instantiate(p_rechts, pos, Quaternion.identity);
                ps.transform.eulerAngles = new Vector3(0, leader.transform.rotation.y, 0);
            }
            else
            {
                ParticleSystem ps = Instantiate(p_links, pos, Quaternion.identity);
                ps.transform.eulerAngles = new Vector3(0, leader.transform.rotation.y, 0);
            }
        }
        else
        {
            if (y > 0)
            {
                ParticleSystem ps = Instantiate(p_oben, pos, Quaternion.identity);
            }
            else
            {
                ParticleSystem ps = Instantiate(p_unten, pos, Quaternion.identity);
            }
        }
    }

    private IEnumerator runTimer()
    {
        yield return new WaitForSeconds(3);
        running = false;
    }
}