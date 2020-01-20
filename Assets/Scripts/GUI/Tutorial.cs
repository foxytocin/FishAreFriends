using System.Collections;
using UnityEngine;

public class Tutorial : MonoBehaviour
{

    GuiOverlay guiOverlay;
    TopStatsScroller topStatsScroller;
    Leader leaderPlayer;

    GameObject aiPlayer;

    void Awake()
    {
        guiOverlay = FindObjectOfType<GuiOverlay>();
        topStatsScroller = FindObjectOfType<TopStatsScroller>();
        leaderPlayer = GameObject.Find("Leader").GetComponent<Leader>();
    }

    void Start()
    {
        StartCoroutine(BasicTutorial());
    }


    private IEnumerator BasicTutorial()
    {

        // start
        yield return new WaitForSeconds(3);
        guiOverlay.DisplayMainMessage("Los geht's", 2, GuiOverlay.MessageType.info);

        // find friends
        yield return new WaitForSeconds(8);
        guiOverlay.DisplayMainMessage("Du bist ganz allein. Finde neue Freunde, indem Du in ihre Naehe schwimmst", 7, GuiOverlay.MessageType.tutorial);

        // wait until 10 friend are found
        int count = 0;
        while (count < 10)
        {
            count = leaderPlayer.GetSwarmSize();
            yield return new WaitForEndOfFrame();
        }
        guiOverlay.DisplayMainMessage("Sehr gut! Du hast " + count + " neue Freunde gefunden", 3, GuiOverlay.MessageType.tutorial);

        // display
        yield return new WaitForSeconds(13);
        topStatsScroller.FadeInTopStats();

        yield return new WaitForSeconds(3);
        guiOverlay.DisplayMainMessage("Die Zahl im Infobereich gibt an, wie gross Dein Schwarm ist", 5, GuiOverlay.MessageType.tutorial);

        // display
        yield return new WaitForSeconds(15);
        guiOverlay.DisplayMainMessage("Das Balkendiagramm zeigt das Verhaeltnis zu allen Fischen im Aquarium", 7, GuiOverlay.MessageType.tutorial);

        // display
        yield return new WaitForSeconds(15);
        guiOverlay.DisplayMainMessage("Das Fischsymbol zeigt Dir, wie hungrig dein Schwarm ist", 4, GuiOverlay.MessageType.tutorial);

        // spawn opponentPlayers
        yield return new WaitForSeconds(15);
        GameObject.Find("Spawner").GetComponent<Spawner>().SpawnOpponentPlayers();
        guiOverlay.DisplayMainMessage("Es sind weitere Fischschwaerme aufgetaucht", 3, GuiOverlay.MessageType.tutorial);

        // display
        yield return new WaitForSeconds(30);
        guiOverlay.DisplayMainMessage("Im Balkendiagramm kannst Du nun deine Schwarmgroesse, mit der der Anderen vergleichen", 8, GuiOverlay.MessageType.tutorial);

        // beware the shark
        yield return new WaitForSeconds(60);
        GameObject.Find("Spawner").GetComponent<Spawner>().SpawnPredators();
        guiOverlay.DisplayMainMessage("Sei vorsichtig! Hier soll es Haie geben", 3, GuiOverlay.MessageType.warning);
    }

}