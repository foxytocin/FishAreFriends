using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

class GameObjectWithTime
{
    public GameObject gameObject;
    public long creationTime;

    public GameObjectWithTime(GameObject gameObject, long creationTime)
    {
        this.gameObject = gameObject;
        this.creationTime = creationTime;
    }
}

public class FlowStream : MonoBehaviour
{
    public Leader leader;
    public GameObject emitter;

    private Queue<GameObjectWithTime> flowObjectEmitter;
    private long lastUpdate = 0;


    public void Awake()
    {

        flowObjectEmitter = new Queue<GameObjectWithTime>();

        for(int i=0; i<20; i++)
        {
            GameObject gameObject = Instantiate(emitter);
            gameObject.SetActive(false);
            flowObjectEmitter.Enqueue(new GameObjectWithTime(gameObject, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));
        }

        StartCoroutine(disableGameObjects());
    }


    public void playFlowStream(Vector3 forward, Vector3 hitpoint)
    {
        instanciateNewFlowEmitter(leader.transform.position, forward);
    }


    private void instanciateNewFlowEmitter(Vector3 position, Vector3 forward)
    {

        long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        if (lastUpdate > now - 2)
            return;
        lastUpdate = now;

        GameObjectWithTime gameObjectWithTime = flowObjectEmitter.Dequeue();

        GameObject gameObject = gameObjectWithTime.gameObject;
        gameObject.transform.position = leader.transform.position;
        gameObject.transform.rotation = Quaternion.LookRotation(forward);
        gameObject.SetActive(true);

        gameObjectWithTime.creationTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        flowObjectEmitter.Enqueue(gameObjectWithTime);

    }

    private IEnumerator disableGameObjects()
    {

        while (true)
        {
            //GameObjectWithTime gameObjectWithTime = flowObjectEmitter.Peek();
            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();


            foreach(GameObjectWithTime gameObjectWithTime in flowObjectEmitter)
            {
                if (gameObjectWithTime.creationTime < now - 5)
                    gameObjectWithTime.gameObject.SetActive(false);
            }


            yield return new WaitForSeconds(3);
        }

        
        
    }

}