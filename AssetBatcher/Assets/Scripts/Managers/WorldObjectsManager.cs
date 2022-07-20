using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectsManager : MonoBehaviour
{
    public List<GameObject> worldObjects{ set; get; }

    public GameObject[] GameObjects;
    // Start is called before the first frame update
    void Start()
    {
        worldObjects.Add(GameObjects[0]);
        worldObjects.Add(GameObjects[1]);
        worldObjects.Add(GameObjects[2]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
