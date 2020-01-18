using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DistributionGraph : MonoBehaviour
{

    public DisplaySegment displaySegmentPrefab;
    public int displayElementsAmount = 2;
    private List<DisplaySegment> displaySegmentsArray;

    public float player1 = 0;
    public float player2 = 0;

    private Color32[] colorArray;

    void Awake()
    {
        displaySegmentsArray = new List<DisplaySegment>();
        colorArray = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255) };
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < displayElementsAmount; i++)
        {
            DisplaySegment seg = Instantiate(displaySegmentPrefab);
            seg.transform.SetParent(this.transform, false);
            seg.SetInitialPosition(new Vector2(-1000, 0));
            seg.SetColor(colorArray[i]);

            displaySegmentsArray.Add(seg);
        }

        Test();
    }

    float posSum = 100;
    void Test()
    {
        for (int i = 0; i < displaySegmentsArray.Count; i++)
        {
            displaySegmentsArray[i].SetPosition(posSum);
            posSum += 100;
        }
    }
}
