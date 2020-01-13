using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{

    GuiOverlay guiOverlay;
    TopStatsScroller topStatsScroller;
    Leader leaderPlayer;
    
    GameObject aiPlayer;

    void Awake() {
        guiOverlay = FindObjectOfType<GuiOverlay>();
        topStatsScroller = FindObjectOfType<TopStatsScroller>();
        leaderPlayer = GameObject.Find("Leader").GetComponent<Leader>();
        aiPlayer = GameObject.Find("Fish_OpponentPlayer");
        aiPlayer.SetActive(false);
    }

    void Start() {
        StartCoroutine(BasicTutorial());
    }



    private IEnumerator BasicTutorial() {

        // start
        yield return new WaitForSeconds(4);
        guiOverlay.DisplayMainMessage("Los geht's", 2);

        // find friends
        yield return new WaitForSeconds(8);
        guiOverlay.DisplayMainMessage("Du bist ganz allein. Finde neue Freunde, indem Du in ihre Naehe schwimmst", 7);

        // wait until 10 friend are found
        int count = 0;
        while(count < 10) {
            count = leaderPlayer.GetSwarmSize();
            yield return new WaitForEndOfFrame();
        }
        guiOverlay.DisplayMainMessage("Sehr gut! Du hast " +count+ " neue Freunde gefunden", 3);


        yield return new WaitForSeconds(10);
        guiOverlay.DisplayMainMessage("Sieh mal nach Oben", 2);
        
        yield return new WaitForSeconds(2);
        topStatsScroller.FadeInTopStats();


        yield return new WaitForSeconds(6);
        guiOverlay.DisplayMainMessage("Dort kannst Du sehen, wie gross Dein Schwarm ist und wieviel Nahrung euch bleibt", 7);
        topStatsScroller.FadeInTopStats();


        aiPlayer.SetActive(true);

        // beware the shark
        yield return new WaitForSeconds(80);
        guiOverlay.DisplayMainMessage("Sei vorsichtig! Hier soll irgendwo ein Hai sein", 8);





    }




}
