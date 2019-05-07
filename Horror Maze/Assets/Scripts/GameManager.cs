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


    //public byte[,] GetMazeArray()
    //{
    //    return generatorScript.mazeArray;
    //}

    /*
    public Maze mazePrefab;
    private Maze mazeInstance;
    // Start is called before the first frame update
    void Start()
    {
        startGame();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            restartGame();
        }
    }

    private void startGame()
    {
        mazeInstance = Instantiate(mazePrefab) as Maze;
        mazeInstance.Generate();
    }
    private void restartGame()
    {
        Destroy(mazeInstance.gameObject);
        startGame();
    }
    */
}
