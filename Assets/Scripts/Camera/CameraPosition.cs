using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraPosition : MonoBehaviour
{

    MapGenerator mapGenerator;

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



    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        myCamera = GetComponent<Camera>();
        volume = postProcessing.GetComponent<Volume>();
    }


    // Start is called before the first frame update
    void Start()
    {
        targetPosition = Side_View;
        centerOfTank = mapGenerator.mapSize / 2;
        centerOfTank += new Vector3(0, -(mapGenerator.mapSize.y / 2), 0) + new Vector3(0, (float)mapGenerator.heightScale, 0);
        //Top_View.position = centerOfTank + new Vector3(0, mapGenerator.mapSize.y * 2, 0);
        lookAtCenterOfTank = centerOfTank;
        originalFogDensity = RenderSettings.fogDensity;
        originalFieldOfView = myCamera.fieldOfView;
        startPosition = transform.position.y;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        bool down = Input.GetKeyDown(KeyCode.Space);
        if (down & side)
        {
            side = false;
            myCamera.farClipPlane = 1000;
            SwitchFieldOfViewToTop(true);
            startPosition = transform.position.y;
            targetPosition = Top_View;
            totalDistanceToTarget = Mathf.Abs(transform.position.y - targetPosition.position.y);
            DisablePostEffects(true);
            SwitchFodDensityToTop(true);

        }
        else if (down & !side)
        {
            side = true;
            SwitchFieldOfViewToTop(false);
            startPosition = transform.position.y;
            lookAtLeader = target.position;
            targetPosition = Side_View;
            totalDistanceToTarget = Mathf.Abs(transform.position.y - lookAtLeader.y);
            DisablePostEffects(false);
            SwitchFodDensityToTop(false);
            StartCoroutine(SetClippingPlane());

        }

        if (targetPosition.position != transform.position)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition.position, 0.3f * Time.deltaTime * 5);
            // float percentOfWay = Mathf.Abs(transform.position.y - startPosition) / totalDistanceToTarget;
            // percentOfWay = Mathf.Clamp(percentOfWay, 0.01f, 1f);

            // if (side)
            // {
            //     //myCamera.fieldOfView = map(percentOfWay, 0, 0.9f, 10f, 60f);
            //     myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, map(percentOfWay, 0.01f, 1f, 10f, 60f), 0.2f * Time.deltaTime * 2);
            // }
            // else
            // {
            //     //myCamera.fieldOfView = map(percentOfWay, 0, 0.9f, 60f, 10f);
            //     myCamera.fieldOfView = map(percentOfWay, 0.01f, 1f, 60f, 10f);

            // }

        }

        if (side)
        {
            lookAtLeader = target.position;

            if (tmpLookAtTarget != lookAtLeader)
            {
                tmpLookAtTarget = Vector3.Lerp(tmpLookAtTarget, lookAtLeader, 0.4f * Time.deltaTime * 5);
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
            //This enables the orthographic mode
            //myCamera.orthographic = true;

            //myCamera.fieldOfView = 10f;
            targetFieldOfView = 10f;
            //StartCoroutine(AnimateFieldOfView());
            //Set the size of the viewing volume you'd like the orthographic Camera to pick up (5)
            //myCamera.orthographicSize = 30f;

            //Set the orthographic Camera Viewport size and position
            //camera.rect = new Rect(centerOfTank.x, centerOfTank.y, mapGenerator.mapSize.x, mapGenerator.mapSize.y);
        }
        else
        {
            //This enables the orthographic mode
            //myCamera.orthographic = false;
            targetFieldOfView = 60f;
            //StartCoroutine(AnimateFieldOfView());
            //myCamera.fieldOfView = 60f;

            //Set the orthographic Camera Viewport size and position
            //camera.rect = new Rect(m_ViewPositionX, m_ViewPositionY, m_ViewWidth, m_ViewHeight); 
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
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, 1f * Time.deltaTime * speed);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (RenderSettings.fogDensity > targetFogDensity)
            {
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, 1f * Time.deltaTime * speed);
                yield return new WaitForEndOfFrame();
            }

            if (RenderSettings.fogDensity < 0)
                RenderSettings.fogDensity = 0;
        }
    }


    private IEnumerator SetClippingPlane()
    {
        yield return new WaitForSeconds(10);
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
            depthOfField.active = true;
        }
    }

}
