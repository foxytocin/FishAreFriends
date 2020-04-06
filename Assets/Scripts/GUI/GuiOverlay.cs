﻿using UnityEngine;
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
    public Button newMapButton;

    private CameraPosition cameraPosition;
    private MenuScrollerRight menuScrollerRight;
    private MenuScrollerLeft menuScrollerLeft;

    private MapGenerator mapGenerator;
    private Spawner spawner;
    private SettingMenu settingMenu;

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
        gameStatus = GameStatus.newGame;
        textMeshPro = FindObjectOfType<TextMeshProUGUI>();
        cameraPosition = FindObjectOfType<CameraPosition>();
        menuScrollerLeft = FindObjectOfType<MenuScrollerLeft>();
        menuScrollerRight = FindObjectOfType<MenuScrollerRight>();
        mapGenerator = FindObjectOfType<MapGenerator>();
        spawner = FindObjectOfType<Spawner>();
        settingMenu = FindObjectOfType<SettingMenu>();
        mainMessagesColor = mainMessages.color;
        mainMessagesColor.a = 1f;
        mainMessagesColorStandard = mainMessages.color;
    }

    void Start()
    {
        NewGame();
    }

    void LateUpdate()
    {
        if (!cameraPosition.toggleView && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
        {
            if (gameStatus == GameStatus.inGame)
            {
                Pause();
            }
            else if (gameStatus == GameStatus.pausedGame)
            {
                Play();
            }
        }
    }

    void NewGame()
    {
        gameStatus = GameStatus.newGame;
        Cursor.visible = true;
        Application.targetFrameRate = 0;

        playButton.onClick.AddListener(PlayButtonClickEvent);
        playButtonText.text = "Start";

        quitButton.onClick.AddListener(QuitButtonClickEvent);
        restartButton.onClick.AddListener(RestartButtonClickEvent);
        newMapButton.onClick.AddListener(GenerateNewMap);

        menuScrollerLeft.FadeIn();
        menuScrollerRight.FadeIn();
    }

    void GenerateNewMap()
    {
        mapGenerator.GenerateMap();
    }


    void PlayButtonClickEvent()
    {
        if (gameStatus == GameStatus.inGame)
        {
            Pause();
            return;
        }
        Play();
    }


    void Play()
    {
        if (gameStatus == GameStatus.newGame)
            spawner.SpawnFishSwarms();

        settingMenu.PlayBubbleSound();
        Cursor.visible = false;
        gameStatus = GameStatus.inGame;
        menuScrollerLeft.FadeOut();
        menuScrollerRight.FadeOut();
    }

    void Pause()
    {
        Cursor.visible = true;
        gameStatus = GameStatus.pausedGame;
        playButtonText.text = "Fortsetzen";

        settingMenu.PlayBubbleSound();
        newMapButton.gameObject.SetActive(false);
        menuScrollerLeft.FadeIn();
        menuScrollerRight.FadeIn();
        //playButton.gameObject.SetActive(false);
    }


    void QuitButtonClickEvent()
    {
        Application.Quit();
    }

    void RestartButtonClickEvent()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void SetPlayerSwarmSize(int size)
    {
        if (size == 0)
            playerSwarmSize.text = "alleine";

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
