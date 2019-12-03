using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityStandardAssets.Characters.FirstPerson;

public class BruteController : MonoBehaviour
{
    public float bruteSpeed = 1;
    public float lookSpeed = 50;

    // -------- Audio ----------
    private AudioSource audio1;
    private AudioSource audio2;
    private AudioSource audio3;

    // -------- Charging customizations --------
    public float chargeSpeed = 10;
    public float chargeCD = 5; // seconds until next charge ability

    // -------- Player detection --------
    public float raycastAngle = 45; // angle of the cone
    public float raycastDistance = 20; // Length of raycast
    public LayerMask layerMask;
    public TrapTrigger trapScript;

    [SerializeField] float coolDown = 5f;

    private GameObject player;
    private CharacterController controller;
    private bool isCharging = false; // is the brute currently in charge mode
    private bool inCoolDown = false; // is the charge ability in cool down
    private float cdTimer = 5; // charge cool down timer
    private Vector3 chargeDir; // the charge direction, brute can't change direction midcharge

    private Seeker seeker;

    // -------- Pathfinding components --------
    public Path path;
    public float nextWaypointDistance = 1;
    public bool reachedEndOfPath = false;
    private int currentWaypoint = 0;

    private MeshRenderer bruteRender;
    private bool respawnCoolDown = false;

    // Start is called before the first frame update
    void Start()
    {
        // Plays monster noises for the duration of the monster's life
        StartMonsterNoise();
        // Get a reference to the Seeker component we added earlier
        seeker = GetComponent<Seeker>();
        // OnPathComplete will be called every time a path is returned to this seeker
        seeker.pathCallback += OnPathComplete;

        controller = GetComponent<CharacterController>();
        cdTimer = chargeCD;

        bruteRender = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Since monsters are capsules right now, they collide with 
        // walls and fall over. This is to keep them upright.
        //transform.eulerAngles = new Vector3(0, 0, 0);

        player = GameManager.instance.GetPlayerGO();

        if (!respawnCoolDown)
            BruteAI();
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

        // Since the path velocity does not take into account
        // of the y axis, the brute will always look at the ground. 
        // Thus, we manually reset the y position. 
        Vector3 lookDir = velocity;
        lookDir.y = transform.forward.y;
        Quaternion lookTarget = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTarget, lookSpeed * Time.deltaTime);
    }

    private bool SendRaycast()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, raycastDistance, layerMask))
        {
            if (hit.transform.gameObject.tag.Equals("Player"))
            {
                Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow);

                // If the brute is not already charging, and if
                // the cool down is not in effect, charge. 
                if (!isCharging && !inCoolDown)
                {
                    EnableChargeMode();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, direction * raycastDistance, Color.white);

            return false;
        }
    }

    private bool PlayerInSight()
    {
        Vector3 direction = player.transform.position - transform.position;

        // If the player is within the cone detection range
        // (i.e. within both angle and distance range) then 
        // preliminary check is passed. 
        if ((Vector3.Angle(direction, transform.forward) <= raycastAngle) && 
            (Vector3.Distance(player.transform.position, transform.position) <= raycastDistance))
        {
            Debug.DrawRay(transform.position, direction, Color.magenta);

            return true;
        }

        return false;
    }

    private void EnableChargeMode()
    {
        Debug.Log("Charge mode activated...");
        //Plays an audio clip every time the brute charges
        audio2.Play();
        isCharging = true;
        Vector3 dir = (player.transform.position - transform.position).normalized;
        Vector3 velocity = dir * chargeSpeed;
        chargeDir = velocity;
    }

    // Finds direct path to player, but travels really slowly.
    // When player is in sight, goes into "charge mode". "Charge
    // mode" has a cooldown of 5? seconds.
    private void BruteAI()
    {
        seeker.StartPath(transform.position, player.transform.position);
        reachedEndOfPath = false;

        // If the player is within the cone of view of the brute, 
        // then send a raycast to the player to make sure the player
        // is not behind a wall. 
        if (PlayerInSight())
        {
            SendRaycast();
        }

        // if the brute is still charging, keep moving
        // towards the player direction. 
        if (isCharging)
        {
            Debug.Log("Still charging my ass off");
            controller.SimpleMove(chargeDir);

            Vector3 lookDir = chargeDir;
            lookDir.y = transform.forward.y;
            Quaternion lookTarget = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTarget, lookSpeed * Time.deltaTime);
        }
        else
        {
            if (inCoolDown)
            {
                cdTimer -= Time.deltaTime;

                // if the cool down timer reaches 0, then brute can
                // charge again. Reset cool down timer.
                if (cdTimer <= 0)
                {
                    inCoolDown = false;
                    cdTimer = chargeCD;

                    Debug.Log("Cool down completed where that bitch player at");
                }
            }

            FollowPath(bruteSpeed);
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag.Equals("Player"))
        {
            StartCoroutine(JumpScare(hit.gameObject));
        }
        // If the brute hits a wall, and is in charge mode, stop
        // charging. Then start cool down timer.
        else if (hit.gameObject.tag.Equals("Wall") && (isCharging))
        {
            Debug.Log("Oof hit a wall");
            isCharging = false;
            inCoolDown = true;
        }
    }


    public IEnumerator JumpScare(GameObject player)
    {

        player.GetComponent<MonsterJumpScare>().Show(3);
        player.transform.GetChild(2).Find("JumpScareLight").gameObject.SetActive(true);
        player.GetComponent<FirstPersonController>().enabled = false;
        //this.gameObject.SetActive(false);
        GetComponentInChildren<MeshRenderer>().enabled = false;

        Time.timeScale = 0;
        Debug.Log("TIME FREEZE");
        //lock camera movement
        yield return new WaitForSecondsRealtime(1);

        Debug.Log("TIME START AGAIN");
        player.transform.GetChild(2).Find("JumpScareLight").gameObject.SetActive(false);
        player.GetComponent<FirstPersonController>().enabled = true;
        Time.timeScale = 1;
        trapScript.Respawn(player, transform.position);
        StartCoroutine(RespawnTimer());
    }

    public IEnumerator RespawnTimer()
    {
        respawnCoolDown = true;
        bruteRender.enabled = false;
        controller.enabled = false;

        yield return new WaitForSeconds(coolDown);

        respawnCoolDown = false;
        bruteRender.enabled = true;
        controller.enabled = true;
    }

    // Starts a monster background noise and then plays a different noise within 100-150 seconds
    private void StartMonsterNoise()
    {
        audio1 = GetComponents<AudioSource>()[0];
        audio2 = GetComponents<AudioSource>()[1];
        audio3 = GetComponents<AudioSource>()[2];
        audio1.Play();
        StartCoroutine(MonsterNoises());
    }
        private IEnumerator MonsterNoises()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(100, 150));
            Debug.Log("playing moan");
            audio2.Play();
        }
    }
}
