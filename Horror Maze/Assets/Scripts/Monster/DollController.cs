﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class DollController : MonoBehaviour
{
    public float dollSpeed = 4;
    public TrapTrigger trapScript;

    private GameObject player;
    private CharacterController controller;
    private static Vector3 deathOrigin;

    // -------- AI components --------
    private List<Transform> trapLocations;
    private int trapIndex = 0;
    private Seeker seeker;

    // -------- Pathfinding components --------
    public Path path;
    public float nextWaypointDistance = 1;
    public bool reachedEndOfPath = false;
    private int currentWaypoint = 0;

    // IComparer used to sort trap transforms. 
    // Comparisons are based on the distance to the 
    // deathOrigin
    class DistComparison : IComparer<Transform>
    {
        int IComparer<Transform>.Compare(Transform x, Transform y)
        {
            float xDistance = Vector3.Distance(x.position, deathOrigin);
            float yDistance = Vector3.Distance(y.position, deathOrigin);

            if (xDistance < yDistance)
                return -1;
            else if (yDistance > xDistance)
                return 1;
            else
                return 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Plays monster noises for the duration of the monster's life
        StartCoroutine(MonsterNoises());
        // Get a reference to the Seeker component we added earlier
        seeker = GetComponent<Seeker>();
        // OnPathComplete will be called every time a path is returned to this seeker
        seeker.pathCallback += OnPathComplete;

        controller = GetComponent<CharacterController>();

        deathOrigin = transform.position;

        // Get all the trap locations in the maze
        trapLocations = GameManager.instance.GetTrapsTransforms();
        DistComparison comparer = new DistComparison();
        trapLocations.Sort(comparer);
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

    // Update is called once per frame
    void Update()
    {
        // Since monsters are capsules right now, they collide with 
        // walls and fall over. This is to keep them upright.
        transform.eulerAngles = new Vector3(0, 0, 0);

        DollAI();
    }

    private void FollowPath(float speed)
    {
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            return;
        }

        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        float distanceToWaypoint; // The distance to the next waypoint in the path
        while (true)
        {
            // Sometimes the path search gets buggy, it detects
            // the monster as in the air, so it tries to find
            // a way down to the ground. This ignores the downard
            // vector.
            Vector3 temp_path = path.vectorPath[currentWaypoint];
            Vector3 temp_pos = transform.position;
            temp_path.y = temp_pos.y = 0;

            distanceToWaypoint = Vector3.Distance(temp_pos, temp_path);
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
        // var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

        // Direction to the next waypoint
        // Normalize it so that it has a length of 1 world unit
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        //Debug.Log("Direction = " + dir);
        // Multiply the direction by our desired speed to get a velocity
        Vector3 velocity = dir * speed; // could multiply by speedFactor above

        // Move the agent using the CharacterController component
        // Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
        controller.SimpleMove(velocity);
    }

    // Patrols the maze in a circuit. The circuit is formed 
    // around trap locations in the maze. Strays from the 
    // circuit when it detects the player is within a 
    // certain range. 
    private void DollAI()
    {
        if (reachedEndOfPath)
        {
            trapIndex += 1;
            trapIndex = trapIndex % trapLocations.Count;
            reachedEndOfPath = false;
        }

        // Start to calculate a new path to the targetPosition object, return the result to the OnPathComplete method.
        // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
        // takes to calculate the path. Usually it is called the next frame.
        seeker.StartPath(transform.position, trapLocations[trapIndex].position);

        FollowPath(dollSpeed);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag.Equals("Player"))
        {
            trapScript.Respawn(hit.gameObject);
        }
    }

    // Plays a monster noise every 7-13 seconds when within range
    private IEnumerator MonsterNoises()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(7, 13));
            GetComponent<AudioSource>().Play();
        }
    }
}