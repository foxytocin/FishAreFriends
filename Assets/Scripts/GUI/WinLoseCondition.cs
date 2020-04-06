using System.Collections;
using UnityEngine;

public class WinLoseCondition : MonoBehaviour
{
     private GuiOverlay guiOverlay;


    public int GlobalFishCount = 100;

    public void PlayerWin()
    {
        print("player win");
        guiOverlay.GameEnd(true);
    }


    public void PlayerLose()
    {
        print("player lose");
        guiOverlay.GameEnd(false);
    }


    private void Awake()
    {
        guiOverlay = FindObjectOfType<GuiOverlay>();
        StartCheckingWinLoseCondition();
    }

    private void Update() {

        if(Input.GetKeyDown(KeyCode.T))
            PlayerLose();

        if(Input.GetKeyDown(KeyCode.Z))
            PlayerWin();
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
