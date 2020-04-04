using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GuiOverlay : MonoBehaviour
{

    private TextMeshProUGUI textMeshPro;
    public TextMeshProUGUI playerSwarmSize;
    public TextMeshProUGUI mainMessages;
    private Color mainMessagesColor;
    private Color mainMessagesColorStandard;
    private float alpha = 1f;
    private float timeToFade = 2;
    private bool broadcastingMessage = false;

    public Button playButton;
    public TextMeshProUGUI playButtonText;
    public Button quitButton;
    public Button restartButton;
    private CameraPosition cameraPosition;
    private bool allowToggle = false;

    public enum MessageType
    {
        tutorial,
        info,
        warning
    }

    public enum GameStatus
    {
        newGame,
        inGame,
        pausedGame
    }

    public GameStatus gameStatus;


    void Awake()
    {
        allowToggle = false;
        gameStatus = GameStatus.newGame;
        textMeshPro = FindObjectOfType<TextMeshProUGUI>();
        cameraPosition = FindObjectOfType<CameraPosition>();
        mainMessagesColor = mainMessages.color;
        mainMessagesColor.a = 1f;
        mainMessagesColorStandard = mainMessages.color;

        NewGame();
    }

    void LateUpdate() {
        if (allowToggle && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))) {
            allowToggle = false;

            if(gameStatus == GameStatus.inGame) {
                Pause();
            } else if(gameStatus == GameStatus.pausedGame) {
                 Play();
            }
        }
    }

    void NewGame() {
        gameStatus = GameStatus.newGame;
        Cursor.visible = true;
        Application.targetFrameRate = 0;

        playButton.onClick.AddListener(PlayButtonClickEvent);
        playButtonText.text = "Start";

        quitButton.onClick.AddListener(QuitButtonClickEvent);
        quitButton.gameObject.SetActive(true);

        restartButton.onClick.AddListener(RestartButtonClickEvent);
        restartButton.gameObject.SetActive(true);

        playButton.gameObject.SetActive(false); 
        StartCoroutine(WaitForCamAnimation());
    }


    void PlayButtonClickEvent()
    {
        // If PLAY / UNPAUSE is clicked
        if(gameStatus == GameStatus.newGame || gameStatus == GameStatus.pausedGame) {
            Play();

        // If PAUSE is clicked
        } else if(gameStatus == GameStatus.inGame) {
           Pause();
        }
    }


    void Play() {
            gameStatus = GameStatus.inGame;
            cameraPosition.toggleView = true;

            playButtonText.text = "Pause";

            quitButton.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(false);
            StartCoroutine(WaitForCamAnimation());
    }

    void Pause() {
            gameStatus = GameStatus.pausedGame;
            cameraPosition.toggleView = true;

            playButtonText.text = "Fortsetzen";

            quitButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);

            playButton.gameObject.SetActive(false);
            StartCoroutine(WaitForCamAnimation());
    }

    // display Play/Pause button after the camera-animation has finished
    private IEnumerator WaitForCamAnimation()
    {
        yield return new WaitForSeconds(4);
        if(gameStatus != GameStatus.inGame)
            playButton.gameObject.SetActive(true);
        
        allowToggle = true;
    }

    void UnPause() {
        Cursor.visible = true;

        playButton.onClick.AddListener(PlayButtonClickEvent);
        playButtonText.text = "Start";

        quitButton.onClick.AddListener(QuitButtonClickEvent);
        quitButton.gameObject.SetActive(true);

        restartButton.onClick.AddListener(RestartButtonClickEvent);
        restartButton.gameObject.SetActive(true);
    }


    void QuitButtonClickEvent()
    {
        Application.Quit();
    }

    void RestartButtonClickEvent()
    {
        NewGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void SetPlayerSwarmSize(int size)
    {
        if (size == 0)
            playerSwarmSize.text = "lonely";

        playerSwarmSize.text = size.ToString();
    }


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
