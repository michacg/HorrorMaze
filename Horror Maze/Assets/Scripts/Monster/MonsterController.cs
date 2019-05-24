﻿using System.Collections;
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
        Destroy(gameObject);
        Instantiate(monsters[monsterType], position, Quaternion.identity);
    }
}