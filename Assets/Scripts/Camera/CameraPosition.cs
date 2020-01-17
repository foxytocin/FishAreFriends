using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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

    bool side = true;

    private Transform targetPosition;
    private Vector3 centerOfTank;
    private Camera myCamera;

    public GameObject postProcessing;
    Volume volume;
    DepthOfField depthOfField;
    private float targetFieldOfView;
    private float originalFieldOfView;
    private float targetFogDensity;
    private float originalFogDensity;

    float totalDistanceToTarget = 1;
    float startPosition = 0;
    Coroutine setClippingPlane = null;

    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        myCamera = GetComponent<Camera>();
        volume = postProcessing.GetComponent<Volume>();
        topStatsScroller = FindObjectOfType<TopStatsScroller>();
        musicMenu = FindObjectOfType<MusicMenu>();
        ambientMusic = FindObjectOfType<AmbientMusic>();
    }


    void Start()
    {
        targetPosition = Side_View;
        centerOfTank = mapGenerator.mapSize / 2;
        centerOfTank += new Vector3(0, -(mapGenerator.mapSize.y / 2), 0) + new Vector3(0, (float)mapGenerator.heightScale, 0);
        lookAtCenterOfTank = centerOfTank + new Vector3(0, 0, 20);
        originalFogDensity = RenderSettings.fogDensity;
        originalFieldOfView = myCamera.fieldOfView;
        startPosition = transform.position.y;
    }


    bool switchingPerspevtiv = false;

    void FixedUpdate()
    {

        bool down = Input.GetKeyDown(KeyCode.Space);

        if (!switchingPerspevtiv && down & side)
        {
            if (setClippingPlane != null)
                StopCoroutine(setClippingPlane);

            side = false;
            switchingPerspevtiv = true;
            myCamera.farClipPlane = 1000;
            SwitchFieldOfViewToTop(true);
            startPosition = transform.position.y;
            targetPosition = Top_View;
            totalDistanceToTarget = Mathf.Abs(transform.position.y - targetPosition.position.y);
            DisablePostEffects(true);
            SwitchFodDensityToTop(true);
            topStatsScroller.FadeOutTopStats();
            musicMenu.StartMenuMusic();
            ambientMusic.StopAmbientMusic();

        }
        else if (!switchingPerspevtiv && down & !side)
        {
            side = true;
            switchingPerspevtiv = true;
            SwitchFieldOfViewToTop(false);
            startPosition = transform.position.y;
            lookAtLeader = target.position;
            targetPosition = Side_View;
            totalDistanceToTarget = Mathf.Abs(transform.position.y - lookAtLeader.y);
            DisablePostEffects(false);
            SwitchFodDensityToTop(false);
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


    private void SwitchFieldOfViewToTop(bool value)
    {
        if (value)
        {
            targetFieldOfView = 10f;
        }
        else
        {
            targetFieldOfView = 60f;
        }
    }

    private void SwitchFodDensityToTop(bool top)
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
        }
        else
        {
            while (RenderSettings.fogDensity > targetFogDensity)
            {
                RenderSettings.fogDensity -= (0.3f * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            switchingPerspevtiv = false;

            if (RenderSettings.fogDensity < 0)
                RenderSettings.fogDensity = 0;
        }
    }


    private IEnumerator SetClippingPlane()
    {
        yield return new WaitForSeconds(5);
        myCamera.farClipPlane = 300;
    }


    private IEnumerator AnimateFieldOfView()
    {
        while (myCamera.fieldOfView != targetFieldOfView)
        {
            myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFieldOfView, 0.3f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }



    private void DisablePostEffects(bool top)
    {

        DepthOfField tempDof;

        if (volume.profile.TryGet<DepthOfField>(out tempDof))
        {
            depthOfField = tempDof;
        }

        if (top)
        {
            //depthOfField.focusDistance.value = 42f;
            depthOfField.active = false;
        }
        else
        {
            //depthOfField.active = true;
        }
    }

}