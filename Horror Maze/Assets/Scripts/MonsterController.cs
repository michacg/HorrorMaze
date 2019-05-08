using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public int numMonsterType = 3;
    public TrapTrigger trapScript;

    // Ghost customization variables.
    public int ghostSpeed = 2;

    private int monsterType = 0;
    private GameObject player;
    

    // Update is called once per frame
    void Update()
    {
        // Detect the number of deaths so far.
        // Transform into a monster by 
        //     (1) choosing a monster type.
        //     (2) changing the prefab from a player dead body to monster body.
        //     (3) run the monstesr AI depending on type.

        // Since monsters are capsules right now, they collide with 
        // walls and fall over. This is to keep them upright.
        transform.eulerAngles = new Vector3(0, 0, 0);
        player = GameManager.instance.GetPlayerGO();

        // Different AI types based on the monster.
        // Case 0: monster is still dormant.
        // Case 1: ghost
        // Case 2: doll
        // Case 3: brute
        switch (monsterType)
        {
            case 0:
                break;

            case 1:
                GhostAI();
                break;

            case 2:
                DollAI();
                break;

            case 3:
                BruteAI();
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            trapScript.Respawn(collision.gameObject);
        }
    }

    public void Transform()
    {
        // Make monster upright. 
        transform.eulerAngles = new Vector3(0, 0, 0);

        monsterType = Random.Range(1, numMonsterType + 1);

        // Different AI types based on the monster.
        // Case 0: monster is still dormant.
        // Case 1: ghost
        // Case 2: doll
        // Case 3: brute
        switch (monsterType)
        {
            case 1:
                gameObject.tag = "Ghost";
                break;

            case 2:
                // Change prefab into ghost prefab. Here, size is changed to show 
                // monster type differences.
                transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                Debug.Log("Doll transformation");

                break;

            case 3:
                transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                Debug.Log("Brute transformation");

                break;
        }
    }

    private void GhostAI()
    {
        transform.position = Vector3.MoveTowards(transform.position, 
            player.transform.position, ghostSpeed * Time.deltaTime);
    }

    private void DollAI()
    {

    }

    private void BruteAI()
    {

    }
}
