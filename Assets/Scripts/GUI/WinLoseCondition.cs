using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseCondition : MonoBehaviour
{
    /* Description:
     * Win condition:
     *  - Player has more than 70% of fishes in his swarm
     *  
     * Lose condition:
     *  - Player has less than 8 fishes, after he has more than 10% of fishes 
     * 
     */

    public int GlobalFishCount = 100;

    public void PlayerWin()
    {
        print("player win");
        // TODO: implement player win stuff what should happen
    }


    public void PlayerLose()
    {
        print("player lose");
        // TODO: implement player lose stuff what should happen
    }


    private void Awake()
    {
        StartCheckingWinLoseCondition();
    }

    public void StartCheckingWinLoseCondition()
    {
        StartCoroutine(Calculate());
    }


    private IEnumerator Calculate()
    {
        bool humanPlayerReached100Fishes = false;
        yield return new WaitForSeconds(30f);


        while (true)
        {

            if (CalculateSwarmSizes.calculatedSwarmSizeList == null)
                yield return new WaitForSeconds(30f);

            print("Check win lose");
            int humanPlayerFishCount = 0;


            humanPlayerFishCount = CalculateSwarmSizes.calculatedSwarmSizeList[CalculateSwarmSizes.calculatedSwarmSizeList.Count -1].size;
            if (humanPlayerFishCount >= GlobalFishCount * 0.1)
            {
                humanPlayerReached100Fishes = true;
                print("human player reached 100 fishes");
            }
                

            if(humanPlayerFishCount <= 8 && humanPlayerReached100Fishes)
            {
                PlayerLose();
                // stop this method
                yield break;
            }

            if(humanPlayerFishCount > GlobalFishCount * 0.7)
            {
                PlayerWin();
                // stop this method
                yield break;
            }
                


            yield return new WaitForSeconds(30f);
        }
    }




}
