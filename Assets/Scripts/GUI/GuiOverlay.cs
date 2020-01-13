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


    void Awake()
    {
        textMeshPro = FindObjectOfType<TextMeshProUGUI>();
        mainMessagesColor = mainMessages.color;
    }

    void Start() {
        DisplayMainMessage("Los geht's");
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


    public void DisplayMainMessage(string message) {

        mainMessages.text = message;

        StartCoroutine(DisplayAndFadeMainMessage());
    }

    float alpha = 0f;
    private IEnumerator DisplayAndFadeMainMessage() {
        
        while (alpha < 1)
        {
            alpha += 0.1f;
            mainMessagesColor.a = alpha;
            mainMessages.color = mainMessagesColor;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(2);

        while (alpha > 0)
        {
            alpha -= 0.1f;
            mainMessagesColor.a = alpha;
            mainMessages.color = mainMessagesColor;
            yield return new WaitForEndOfFrame();
        }

        alpha = 0;
        mainMessages.text = "";
    }
}
