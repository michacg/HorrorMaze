using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public IntVector2 size;
    private MazeCell[,] cells;
    public MazeCell cellPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Generate()
    {
        cells = new MazeCell[size.x, size.z];
        for(int x = 0; x < size.x; x++)
        {
            for(int z = 0; z < size.z; z++)
            {
                CreateCell(new IntVector2(x, z));
            }
        }
    }
    private void CreateCell(IntVector2 coordinates)
    {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x,coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
        newCell.transform.parent = transform;   //programmatically instantiates the object as a child of the right variable
        newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
    }
}
