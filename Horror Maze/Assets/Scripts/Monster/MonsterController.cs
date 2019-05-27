using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MonsterController : MonoBehaviour
{
    public GameObject[] monsters;

    public void Transform()
    {
        // Make monster upright. 
        transform.eulerAngles = new Vector3(0, 0, 0);

        int monsterType = 1; // debugging AI purposes
        //int monsterType = Random.Range(0, monsters.Length);
        Vector3 position = transform.position;
        position.y = 5;
        Debug.Log("Origin = " + position);

        Destroy(gameObject);
        GameObject go = Instantiate(monsters[monsterType], position, Quaternion.identity);

        // If the monster is ghost, then add it to the ghost 
        // list in GameManager. So that it walls can more 
        // efficiently find ghost objects, and ignore collisions.
        if (monsterType == 0)
        {
            GameManager.instance.AddGhost(go);
        }
    }
}
