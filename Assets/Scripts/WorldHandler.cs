using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldHandler : MonoBehaviour
{

    public int worldX = 150;
    public int worldZ = 150;
    public int worldHeight = 50;

    List<Fish>[,] fishList;


    #region singelton
    public static WorldHandler Instance;
    #endregion singelton

    private void Awake()
    {
        Instance = this;
        fishList = new List<Fish>[worldX / 10, worldZ / 10];
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
