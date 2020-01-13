using UnityEngine;
using TMPro;
using System.Collections;

public class GuiOverlay : MonoBehaviour
{
    
    private TextMeshProUGUI textMeshPro;
    public TextMeshProUGUI playerEnergie;
    public TextMeshProUGUI playerSwarmSize;
    public TextMeshProUGUI mainMessages;
    private Color mainMessagesColor;
    private float alpha = 1f;
    private float timeToFade = 2;

    void Awake()
    {
        textMeshPro = FindObjectOfType<TextMeshProUGUI>();
        mainMessagesColor = mainMessages.color;
        mainMessagesColor.a = 1f;
    }


    public void SetPlayerEnergie(int energie)
    {
        playerEnergie.text = energie.ToString();
    }

    public void SetPlayerSwarmSize(int size)
    {
        if(size == 0)
            playerSwarmSize.text = "lonely";
            
        playerSwarmSize.text = size.ToString();
    }


    public void DisplayMainMessage(string message, int timeToFade_) {

        timeToFade = timeToFade_;
        mainMessages.text = message;

        StartCoroutine(DisplayAndFadeMainMessage());
    }


    private IEnumerator DisplayAndFadeMainMessage() {
        
        while (alpha < 1)
        {
            alpha += (1f * Time.deltaTime);
            mainMessagesColor.a = alpha;
            mainMessages.color = mainMessagesColor;
            yield return new WaitForEndOfFrame();
        }

        alpha = 1f;
        yield return new WaitForSeconds(timeToFade);

        while (alpha > 0)
        {
            alpha -= (1f * Time.deltaTime);
            mainMessagesColor.a = alpha;
            mainMessages.color = mainMessagesColor;
            yield return new WaitForEndOfFrame();
        }

        alpha = 0;
        mainMessages.text = "";
    }
}
