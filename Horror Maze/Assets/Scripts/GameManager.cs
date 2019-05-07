using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private Generator generatorScript;


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
    }
    
    public int[] GetMazeSize()
    {
        int[] result = new int[2];

        result[0] = generatorScript.rows;
        result[1] = generatorScript.cols;

        return result;
    }
}
