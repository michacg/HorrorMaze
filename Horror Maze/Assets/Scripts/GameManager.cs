using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int monsterTransformationTurns = 5;

    private Generator generatorScript;

    // Variables related to Monster transformation.
    private List<GameObject> monsterList = new List<GameObject>();
    private GameObject player;
    private int deaths = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);

        generatorScript = gameObject.GetComponent<Generator>();

        AstarPath.active.Scan();
    }
    
    public int[] GetMazeSize()
    {
        int[] result = new int[2];

        result[0] = generatorScript.rows;
        result[1] = generatorScript.cols;

        return result;
    }

    // Get Player GameObject. Since the player is destroyed and
    // cloned at a new location. Player GameObject is always changing. 
    // The monsters need to get the newest Player GameObject, and they
    // can do it by accessing the GameManager.
    // As opposed to using FindGameObjectWithTag, this is more 
    // computationally efficient. 
    public GameObject GetPlayerGO()
    {
        return player;
    }

    public void SetPlayerGO(GameObject new_player)
    {
        player = new_player;
    }

    public void IncrementDeath(GameObject monsterBody)
    {
        monsterList.Add(monsterBody);
        deaths++;

        if (deaths >= monsterTransformationTurns)
        {
            monsterList[0].GetComponent<MonsterController>().Transform();
            monsterList.RemoveAt(0);
            deaths = 0;
        }
    }

    public void endGame()
    {
        Time.timeScale = 0;
    }

    public List<Transform> GetTrapsTransforms()
    {
        List<GameObject> traps = generatorScript.trapList;
        List<Transform> result = new List<Transform>();

        foreach (GameObject go in traps)
        {
            result.Add(go.transform);
        }

        return result;
    }
}
