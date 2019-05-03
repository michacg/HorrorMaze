using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int maze_rows;
    private int maze_cols;

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

        Generator generator = gameObject.GetComponent<Generator>();
        maze_rows = generator.rows;
        maze_cols = generator.cols;
    }

    public int[] GetMazeSize()
    {
        int[] result = new int[2];
        result[0] = maze_rows;
        result[1] = maze_cols;

        return result;
    }

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
