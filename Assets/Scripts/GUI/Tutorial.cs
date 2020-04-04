using UnityEngine;

public class Tutorial : MonoBehaviour
{
    GuiOverlay guiOverlay;
    TopStatsScroller topStatsScroller;
    Leader leaderPlayer;

    public bool reachedTopStatsStep = false;
    public bool showTutorial = true;
    private float passedTime = 0;
    private int step = 1;

    void Awake()
    {
        guiOverlay = FindObjectOfType<GuiOverlay>();
        topStatsScroller = FindObjectOfType<TopStatsScroller>();
        leaderPlayer = GameObject.Find("Leader").GetComponent<Leader>();
    }


    void LateUpdate() {

        if(showTutorial) {
            if(guiOverlay.gameStatus == GuiOverlay.GameStatus.inGame) {
                passedTime += Time.deltaTime;
            } else {
                passedTime = 0;

                // rerdisplays find-friend-step if paused before the swarm is >= 10 fishes
                if(step == 3)
                    step--;
            }

            switch(step) {
                // start
                case 1:
                    if(passedTime > 6) {
                        passedTime = 0;
                        step++;
                        guiOverlay.DisplayMainMessage("Los geht's", 2, GuiOverlay.MessageType.info);
                    
                    }
                break;

                // find friends
                case 2:
                if(passedTime > 8) {
                    guiOverlay.DisplayMainMessage("Du bist ganz allein. Finde neue Freunde, indem Du in ihre Naehe schwimmst", 7, GuiOverlay.MessageType.tutorial);
                    step++;
                }
                break;

                // wait until 10 friend are found
                case 3:
                    int count = leaderPlayer.GetSwarmSize();
                    if(count >= 10) {
                        passedTime = 0;
                        step++;
                        guiOverlay.DisplayMainMessage("Sehr gut! Du hast " + count + " neue Freunde gefunden", 3, GuiOverlay.MessageType.tutorial);
                    }
                break;

                // show too-stats-display
                case 4:
                    if(passedTime > 13) {
                        passedTime = 0;
                        step++;
                        reachedTopStatsStep = true;
                        topStatsScroller.FadeInTopStats();                    
                    }
                break;

                case 5:
                    if(passedTime > 4) {
                        passedTime = 0;
                        step++;
                        guiOverlay.DisplayMainMessage("Die Zahl im Infobereich gibt an, wie gross Dein Schwarm ist", 5, GuiOverlay.MessageType.tutorial);
                    }
                break;

                // display
                case 6:
                    if(passedTime > 15) {
                        passedTime = 0;
                        step++;
                        guiOverlay.DisplayMainMessage("Das Balkendiagramm zeigt das Verhaeltnis zu allen Fischen im Aquarium", 7, GuiOverlay.MessageType.tutorial);
                    }
                break;

                // display
                case 7:
                    if(passedTime > 15) {
                        passedTime = 0;
                        step++;
                        guiOverlay.DisplayMainMessage("Das Fischsymbol zeigt Dir, wie hungrig dein Schwarm ist", 4, GuiOverlay.MessageType.tutorial);
                    }
                break;

                // spawn opponentPlayers
                case 8:
                    if(passedTime > 15) {
                        passedTime = 0;
                        step++;
                        GameObject.Find("Spawner").GetComponent<Spawner>().SpawnOpponentPlayers();
                        guiOverlay.DisplayMainMessage("Es sind weitere Fischschwaerme aufgetaucht", 3, GuiOverlay.MessageType.tutorial);
                    }
                break;

                // display
                case 9:
                    if(passedTime > 30) {
                        passedTime = 0;
                        step++;
                        guiOverlay.DisplayMainMessage("Im Balkendiagramm kannst Du nun deine Schwarmgroesse, mit der der Anderen vergleichen", 8, GuiOverlay.MessageType.tutorial);
                    }
                break;

                // beware of the shark
                case 10:
                    if(passedTime > 60) {
                        passedTime = 0;
                        GameObject.Find("Spawner").GetComponent<Spawner>().SpawnPredators();
                        guiOverlay.DisplayMainMessage("Sei vorsichtig! Hier soll es Haie geben", 3, GuiOverlay.MessageType.warning);

                        // end of tutorial
                        showTutorial = false;
                    }
                break;
                default: break;  
            }
        }
    }


}