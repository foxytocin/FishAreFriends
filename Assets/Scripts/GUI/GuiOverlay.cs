using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GuiOverlay : MonoBehaviour
{

    private TextMeshProUGUI textMeshPro;
    public TextMeshProUGUI playerEnergie;
    public TextMeshProUGUI playerSwarmSize;
    public TextMeshProUGUI mainMessages;
    public TextMeshProUGUI debugInfos;
    private Color mainMessagesColor;
    private Color mainMessagesColorStandard;
    private float alpha = 1f;
    private float timeToFade = 2;
    private bool broadcastingMessage = false;

    public Button playButton;
    public TextMeshProUGUI playButtonText;

    public Button quitButton;

    public Button restartButton;


    public enum MessageType
    {
        tutorial,
        info,
        warning
    }

    void Awake()
    {
        textMeshPro = FindObjectOfType<TextMeshProUGUI>();
        mainMessagesColor = mainMessages.color;
        mainMessagesColor.a = 1f;
        mainMessagesColorStandard = mainMessages.color;

        Cursor.visible = true;

        playButton.onClick.AddListener(PlayButtonClickEvent);
        playButtonText.text = "Pause";

        quitButton.onClick.AddListener(QuitButtonClickEvent);
        quitButton.gameObject.SetActive(false);

        restartButton.onClick.AddListener(RestartButtonClickEvent);
        restartButton.gameObject.SetActive(false);

    }

    void PlayButtonClickEvent()
    {
        playButtonText.text = "Fortsetzen";
        //playButtonText.text = "Neustarten";
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        quitButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    void QuitButtonClickEvent()
    {
        Application.Quit();
    }

    void RestartButtonClickEvent()
    {
        quitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    // public void SetPlayerEnergie(int energie)
    // {
    //     playerEnergie.text = energie.ToString();
    // }

    public void SetPlayerSwarmSize(int size)
    {
        if (size == 0)
            playerSwarmSize.text = "lonely";

        playerSwarmSize.text = size.ToString();
    }

    // public void SetDebugInfo(string text)
    // {
    //     debugInfos.text = text;
    // }

    public void DisplayMainMessage(string message, int timeToFade_, MessageType type)
    {
        if (!broadcastingMessage)
        {
            switch (type)
            {
                case MessageType.info:
                    mainMessagesColor = new Color(255, 255, 0, 0);
                    break;
                case MessageType.tutorial:
                    mainMessagesColor = mainMessagesColorStandard;
                    break;
                case MessageType.warning:
                    mainMessagesColor = new Color(255, 0, 0, 0);
                    break;
                default:
                    break;
            }

            timeToFade = timeToFade_;
            mainMessages.text = message;

            StartCoroutine(DisplayAndFadeMainMessage());
        }
    }


    private IEnumerator DisplayAndFadeMainMessage()
    {
        broadcastingMessage = true;

        alpha = 0;
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

        broadcastingMessage = false;
    }
}
