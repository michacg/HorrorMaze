using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    // int representing different monster types. 
    // monsterType == 0: ghost
    // monsterType == 1: doll
    // monsterType == 2: brute
    public int monsterType = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Different AI types based on the monster
        switch (monsterType)
        {
            case 0:
                GhostAI();
                break;

            case 1:
                DollAI();
                break;

            case 2:
                BruteAI();
                break;
        }
    }

    private void GhostAI()
    {

    }

    private void DollAI()
    {

    }

    private void BruteAI()
    {

    }
}
