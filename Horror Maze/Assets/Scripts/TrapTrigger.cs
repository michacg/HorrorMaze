using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = player.transform.position + new Vector3(0, 0, 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            // Move respawn code from FirstPersonController here. 
            Debug.Log("Replacing current body with monster body");

            Debug.Log("Respawning player in a new location");
            Respawn(other.gameObject);
        }
    }

    private void Respawn(GameObject other)
    {
        // Get maze instance information.
        int[] size = GameManager.instance.GetMazeSize();
        byte[,] mazeArray = Generator.mazeArray;

        // Forward definition for variables used inside
        // the switch-case block.
        List<int[]> empty_cells = new List<int[]>();

        // align is used to figure out which side the player will be
        // respawned at.
        int align = Random.Range(0, 4);
        switch (align)
        {
            // align == 0: player respawns on left side
            case 0:
                empty_cells = FindEmptyCells(false, true);
                break;

            // align == 1: player respawns on right side
            case 1:
                empty_cells = FindEmptyCells(false, false);
                break;

            // align == 2: player respanws on top side
            case 2:
                empty_cells = FindEmptyCells(true, true);
                break;

            // align == 3: player respawns on bottom side
            case 3:
                empty_cells = FindEmptyCells(true, false);
                break;
        }

        int index = Random.Range(0, empty_cells.Count);
        Debug.Log(empty_cells + "; " + index);
        int[] temp = empty_cells[index];

        // Define a new position to spawn the player.
        float new_y = transform.position.y;
        Vector3 new_position = new Vector3(temp[0], new_y, temp[1]);

        // Instantiate new Player at new position
        Instantiate(other, new_position, Quaternion.identity);

        // Replace this body with a monster gameobject
        Destroy(other);
    }

    private List<int[]> FindEmptyCells(bool row_restricted, bool start_restircted)
    {
        int[] size = GameManager.instance.GetMazeSize();
        byte[,] mazeArray = Generator.mazeArray;
        List<int[]> result = new List<int[]>();

        // Searching for empty cells on top or bottom side
        if (row_restricted)
        {
            // Search for empty cells on top side,
            // else search for empty cells on bottom side.
            int index = 1;
            if (!start_restircted)
            {
                index = size[1] - 2;
            }

            while (result.Count == 0 && index >= 1 && index <= size[1] - 2)
            {
                for (int i = 1; i < size[0] - 2; ++i)
                {
                    if (mazeArray[i, index] == 0)
                    {
                        result.Add(new int[] { i , index });
                    }
                }

                if (start_restircted)
                    ++index;
                else
                    --index;
            }
        }

        // Otherwise search for empty cells on right or left side
        else
        {
            // Search for empty cells on left side, 
            // else search for empty cells on right side.
            int index = 1;
            if (!start_restircted)
            {
                index = size[0] - 2;
            }

            while (result.Count == 0 && index >= 1 && index <= size[0] - 2)
            {
                for (int i = 1; i < size[1] - 2; ++i)
                {
                    if (mazeArray[index, i] == 0)
                    {
                        // Result is adjusted to the wacked up maze generation in scene.
                        result.Add(new int[] { index , i });
                    }
                }

                if (start_restircted)
                    ++index;
                else
                    --index;
            }
        }

        return result;
    }
}
