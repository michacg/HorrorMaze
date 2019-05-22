using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using Pathfinding;

public class MonsterController : MonoBehaviour
{
    public int numMonsterType = 3;
    public TrapTrigger trapScript;

    // Ghost customization variables.
    public float ghostSpeed = 2;

    // Doll customization variables
    public float dollSpeed = 4;

    // Brute customization variables
    public float bruteSpeed = 1;

    private int monsterType = 0;
    private GameObject player;

    // AI components
    private Seeker seeker;
    private CharacterController controller;

    public Path path;
    public float nextWaypointDistance = 1;
    private int currentWaypoint = 0;
    public bool reachedEndOfPath;

    private void Start()
    {
        // Get a reference to the Seeker component we added earlier
        seeker = GetComponent<Seeker>();
        // OnPathComplete will be called every time a path is returned to this seeker
        seeker.pathCallback += OnPathComplete;

        controller = GetComponent<CharacterController>();
    }

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

        // Start to calculate a new path to the targetPosition object, return the result to the OnPathComplete method.
        // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
        // takes to calculate the path. Usually it is called the next frame.
        seeker.StartPath(transform.position, player.transform.position);

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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag.Equals("Player"))
        {
            trapScript.Respawn(hit.gameObject);
        }
    }

    public void Transform()
    {
        // Make monster upright. 
        transform.eulerAngles = new Vector3(0, 0, 0);

        monsterType = 1; // debugging AI purposes
        //monsterType = Random.Range(1, numMonsterType + 1);

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

    public void OnPathComplete(Path p)
    {
        Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);

        if (!p.error)
        {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
    }

    public void OnDisable()
    {
        seeker.pathCallback -= OnPathComplete;
    }

    private void GhostAI()
    {
        transform.position = Vector3.MoveTowards(transform.position, 
            player.transform.position, ghostSpeed * Time.deltaTime);
        Vector3 dir = (player.transform.position - transform.position).normalized;
        Vector3 velocity = dir * ghostSpeed;
        controller.SimpleMove(velocity);
    }

    private void DollAI()
    {
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            return;
        }

        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        reachedEndOfPath = false;
        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        while (true)
        {
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            Debug.Log("Distance to Waypoint: " + distanceToWaypoint);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        // Slow down smoothly upon approaching the end of the path
        // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
        //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

        // Direction to the next waypoint
        // Normalize it so that it has a length of 1 world unit
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        //Debug.Log("Direction = " + dir);
        // Multiply the direction by our desired speed to get a velocity
        Vector3 velocity = dir * dollSpeed; // could multiply by speedFactor above

        // Move the agent using the CharacterController component
        // Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
        controller.SimpleMove(velocity);
    }

    private void BruteAI()
    {
        byte[,] mazeArray = Generator.mazeArray;
        
        Vector3 current_location = new Vector3(Mathf.RoundToInt(transform.position.z), 0.5f, Mathf.RoundToInt(transform.position.x));
        Vector3 position_difference = current_location - player.transform.position;
    }
}
