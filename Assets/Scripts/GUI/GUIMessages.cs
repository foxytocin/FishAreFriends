using UnityEngine;
using System.Collections;

public class GUIMessages : MonoBehaviour
{

    string textToDisplay = "";
    float alpha = 0;
    private GUIStyle guiStyle = new GUIStyle();

    void Start()
    {
        guiStyle.normal.textColor = new Color(255, 255, 255, alpha);
        guiStyle.fontSize = 60;
        guiStyle.alignment = TextAnchor.UpperCenter;
        SetText("Los geht's");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, Screen.height / 4 - 25, Screen.width, 50), textToDisplay, guiStyle);
    }

    public void SetText(string text)
    {
        textToDisplay = text;
        StartCoroutine(SetAndRemoveText());
    }

    private IEnumerator SetAndRemoveText()
    {
        while (alpha < 1)
        {
            alpha += 0.1f;
            guiStyle.normal.textColor = new Color(255, 255, 255, alpha);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(2);

        while (alpha > 0)
        {
            alpha -= 0.1f;
            guiStyle.normal.textColor = new Color(255, 255, 255, alpha);
            yield return new WaitForEndOfFrame();
        }

        alpha = 0;
        textToDisplay = "";
    }

}
