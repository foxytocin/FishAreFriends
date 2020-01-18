using UnityEngine;
using System.Collections.Generic;

public class DistributionGraph : MonoBehaviour
{
    public DisplaySegment displaySegmentPrefab;
    private List<DisplaySegment> displaySegmentsArray;
    private int existingElements = 0;


    void Awake()
    {
        displaySegmentsArray = new List<DisplaySegment>();
    }


    void GenerateElement(Color color)
    {
        DisplaySegment seg = Instantiate(displaySegmentPrefab);

        seg.transform.SetParent(this.transform, false);
        seg.transform.SetSiblingIndex(1);
        seg.SetInitialPosition(new Vector2(-1000, 0));
        seg.SetColor((Color32)color);

        displaySegmentsArray.Add(seg);
    }


    void LateUpdate()
    {
        if (CalculateSwarmSizes.calculatedSwarmSizeList.Count > 0)
        {
            if (CalculateSwarmSizes.calculatedSwarmSizeList.Count > existingElements)
            {
                existingElements++;
                int elementToCreate = CalculateSwarmSizes.calculatedSwarmSizeList.Count - existingElements;
                GenerateElement(CalculateSwarmSizes.calculatedSwarmSizeList[elementToCreate].color);
            }
        }


        if (displaySegmentsArray.Count > 0)
        {
            float posSum = -1000;
            for (int i = displaySegmentsArray.Count - 1; i >= 0; i--)
            {
                posSum += CalculateSwarmSizes.calculatedSwarmSizeList[i].size;
                displaySegmentsArray[displaySegmentsArray.Count - 1 - i].SetPosition(posSum);
            }
        }
    }
}
