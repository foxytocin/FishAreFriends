﻿using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class CameraPosition : MonoBehaviour
{
    MusicMenu musicMenu;
    AmbientMusic ambientMusic;
    MapGenerator mapGenerator;
    TopStatsScroller topStatsScroller;
    public Transform Top_View;
    public Transform Side_View;
    public Transform target;
    Vector3 tmpLookAtTarget;
    Vector3 lookAtLeader;
    Vector3 lookAtCenterOfTank;

    bool side = false;

    private Transform targetPosition;
    private Vector3 centerOfTank;
    private Camera myCamera;

    public GameObject postProcessing;
    Volume volume;

    private float targetFogDensity;
    private float originalFogDensity;

    float totalDistanceToTarget = 1;
    float startPosition = 0;
    Coroutine setClippingPlane = null;

    bool switchingPerspevtiv = false;
    public bool toggleView = false;
    private GuiOverlay guiOverlay;

    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        myCamera = GetComponent<Camera>();
        volume = postProcessing.GetComponent<Volume>();
        topStatsScroller = FindObjectOfType<TopStatsScroller>();
        musicMenu = FindObjectOfType<MusicMenu>();
        ambientMusic = FindObjectOfType<AmbientMusic>();
        guiOverlay = FindObjectOfType<GuiOverlay>();

        // start at top
        side = false;
        toggleView = false;
        targetPosition = Top_View;
        switchingPerspevtiv = true;
        myCamera.farClipPlane = 1000;
        totalDistanceToTarget = Mathf.Abs(transform.position.y - targetPosition.position.y);
        SwitchFogDensityToTop(true);
        topStatsScroller.FadeOutTopStats();
        musicMenu.StartMenuMusic();
        ambientMusic.StopAmbientMusic();
    }

    void Start()
    {
        centerOfTank = mapGenerator.mapSize / 2;
        centerOfTank += new Vector3(0, -(mapGenerator.mapSize.y / 2), 0) + new Vector3(0, (float)mapGenerator.heightScale, 0);
        lookAtCenterOfTank = centerOfTank + new Vector3(0, 0, 20);
        originalFogDensity = 0.025f;
        startPosition = transform.position.y;
    }


    void LateUpdate()
    {
        if (!toggleView && guiOverlay.gameStatus == GuiOverlay.GameStatus.inGame && side == false)
        {
            toggleView = true;
            side = true;
        }

        if (!toggleView && guiOverlay.gameStatus != GuiOverlay.GameStatus.inGame && guiOverlay.gameStatus != GuiOverlay.GameStatus.gameEnd && side == true)
        {
            toggleView = true;
            side = false;
        }


        if (!switchingPerspevtiv && toggleView && !side)
        {
            if (setClippingPlane != null)
                StopCoroutine(setClippingPlane);

            switchingPerspevtiv = true;
            myCamera.farClipPlane = 1000;
            startPosition = transform.position.y;
            targetPosition = Top_View;
            totalDistanceToTarget = Mathf.Abs(transform.position.y - targetPosition.position.y);
            SwitchFogDensityToTop(true);
            topStatsScroller.FadeOutTopStats();
            musicMenu.StartMenuMusic();
            ambientMusic.StopAmbientMusic();

        }
        else if (!switchingPerspevtiv && toggleView && side)
        {
            switchingPerspevtiv = true;
            startPosition = transform.position.y;
            lookAtLeader = target.position;
            targetPosition = Side_View;
            totalDistanceToTarget = Mathf.Abs(transform.position.y - lookAtLeader.y);
            SwitchFogDensityToTop(false);
            setClippingPlane = StartCoroutine(SetClippingPlane());
            topStatsScroller.FadeInTopStats();
            musicMenu.StopMenuMusic();
            ambientMusic.StartAmbientMusic();

        }

        if (targetPosition.position != transform.position)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition.position, 0.2f * Time.deltaTime * 5);
        }

        if (side)
        {
            lookAtLeader = target.position + new Vector3(0, 1.25f, 0);

            if (tmpLookAtTarget != lookAtLeader)
            {
                tmpLookAtTarget = Vector3.Lerp(tmpLookAtTarget, lookAtLeader, 0.2f * Time.deltaTime * 5);
                transform.LookAt(tmpLookAtTarget);
            }
        }
        else
        {
            if (tmpLookAtTarget != lookAtCenterOfTank)
            {
                tmpLookAtTarget = Vector3.Lerp(tmpLookAtTarget, lookAtCenterOfTank, 0.4f * Time.deltaTime * 5);
                transform.LookAt(tmpLookAtTarget);
            }
            else
            {
                transform.LookAt(lookAtCenterOfTank);
                transform.TransformDirection(Vector3.down);
            }
        }
    }


    private void SwitchFogDensityToTop(bool top)
    {
        StopCoroutine(AnimateFogDensity());

        if (top)
        {
            targetFogDensity = 0f;
        }
        else
        {
            RenderSettings.fogDensity = 0;
            targetFogDensity = originalFogDensity;
        }

        StartCoroutine(AnimateFogDensity());
    }


    private IEnumerator AnimateFogDensity()
    {
        float speed = (side) ? 0.05f : 0.5f;

        if (side)
        {
            yield return new WaitForSeconds(1);
            while (RenderSettings.fogDensity < targetFogDensity)
            {
                RenderSettings.fogDensity += (0.015f * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            switchingPerspevtiv = false;
            toggleView = false;
        }
        else
        {
            while (RenderSettings.fogDensity > targetFogDensity)
            {
                RenderSettings.fogDensity -= (0.3f * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            if (RenderSettings.fogDensity < 0)
                RenderSettings.fogDensity = 0;

            switchingPerspevtiv = false;
            toggleView = false;
        }
    }


    private IEnumerator SetClippingPlane()
    {
        yield return new WaitForSeconds(5);
        myCamera.farClipPlane = 300;
    }
}