﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    public GameObject monsterPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Respawn(other.gameObject);
        }
    }

    // Respawn is made public so that the MonsterController
    // script can reuse the player respawn code.
    public void Respawn(GameObject other)
    {
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
        int[] temp = empty_cells[index];

        // Define a new position to spawn the player.
        Vector3 new_position = new Vector3(temp[1], 0.7f, temp[0]);

        // Instantiate new Player at new position, and set it as the player 
        // in GameManager.
        GameObject new_player = Instantiate(other, new_position, Quaternion.identity);
        GameManager.instance.SetPlayerGO(new_player);

        // Delete the player GameObject.
        Destroy(other);

        // Replace this body with a monster gameobject.
        Vector3 old_position = transform.position;

        // Quaternion Euler here is used to make the dead body 
        // lie in horizontal position to represent a dead body.
        GameObject monster = Instantiate(monsterPrefab, old_position, Quaternion.Euler(-90f, 0f, 0f));

        GameManager.instance.IncrementDeath(monster);
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