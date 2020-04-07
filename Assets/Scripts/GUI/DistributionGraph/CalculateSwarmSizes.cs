using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct SwarmInformation
{
    public int size;
    public Color color;
}

public class CalculateSwarmSizes : MonoBehaviour
{

    public static List<SwarmInformation> calculatedSwarmSizeList;

    private void Awake()
    {
        calculatedSwarmSizeList = new List<SwarmInformation>();
        StartCoroutine(Calculate());
    }


    private IEnumerator Calculate()
    {
        Leader humanPlayer = null;
        while (true)
        {
            if (Leader.leaderList != null)
            {
                humanPlayer = null;
                calculatedSwarmSizeList.Clear();
                foreach (Leader leader in Leader.leaderList)
                {
                    if (!leader.LeaderIsHumanPlayer())
                    {
                        calculatedSwarmSizeList.Add(new SwarmInformation
                        {
                            color = leader.leaderColor1,
                            size = leader.GetSwarmSize()
                        });
                    }
                    else
                    {
                        humanPlayer = leader;
                    }

                }

                if (humanPlayer != null)
                {
                    calculatedSwarmSizeList.Add(new SwarmInformation
                    {
                        color = humanPlayer.leaderColor1,
                        size = humanPlayer.GetSwarmSize()
                    });
                }
            }

            yield return new WaitForSeconds(2);
        }
    }
}
