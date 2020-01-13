using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{

    GuiOverlay guiOverlay;
    Leader leaderPlayer;

    void Awake() {
        guiOverlay = FindObjectOfType<GuiOverlay>();
        leaderPlayer = GameObject.Find("Leader").GetComponent<Leader>();
    }

    void Start() {
        StartCoroutine(BasicTutorial());
    }



    private IEnumerator BasicTutorial() {

        // splash
        guiOverlay.DisplayMainMessage("FISH ARE FRIENDS", 2);

        // start
        yield return new WaitForSeconds(4);
        guiOverlay.DisplayMainMessage("Los geht's", 2);

        // find friends
        yield return new WaitForSeconds(8);
        guiOverlay.DisplayMainMessage("Du bist ganz allein. Finde neue Freunde, indem Du in ihre Naehe schwimmst", 8);

        // wait until 10 friend are found
        int count = 0;
        while(count < 10) {
            count = leaderPlayer.GetSwarmSize();
            yield return new WaitForEndOfFrame();
        }
        guiOverlay.DisplayMainMessage("Sehr gut! Du hast " +count+ " neue Freunde eingesammelt", 6);

        // beware the shark
        yield return new WaitForSeconds(80);
        guiOverlay.DisplayMainMessage("Sei vorsichtig! Hier soll irgendwo ein Hai sein", 8);





    }




}
