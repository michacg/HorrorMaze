using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public float ghostSpeed = 2;
    public TrapTrigger trapScript;
    public float lookSpeed = 100;

    private CharacterController controller;
    private GameObject player;
    private Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        // Plays monster noises for the duration of the monster's life
        StartCoroutine(MonsterNoises());

        controller = GetComponent<CharacterController>();
        origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        player = GameManager.instance.GetPlayerGO();

        GhostAI();
    }

    private void GhostAI()
    {
        // Find direction towards player
        transform.position = Vector3.MoveTowards(transform.position,
            player.transform.position, ghostSpeed * Time.deltaTime);
        Vector3 dir = (player.transform.position - transform.position).normalized;

        // Move towards player
        Vector3 velocity = dir * ghostSpeed;
        controller.SimpleMove(velocity);

        Vector3 lookDir = velocity;
        lookDir.y = transform.forward.y;
        Quaternion lookTarget = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTarget, lookSpeed * Time.deltaTime);
    }

    // Ghost restarts from its original starting point. This
    // function is called by EnemyDetection script when player
    // shines light on the ghost.
    public void Restart()
    {
        // Needs to disable the character controller during teleportation.
        controller.enabled = false;
        controller.transform.position = origin;
        controller.enabled = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag.Equals("Player"))
        {
            trapScript.Respawn(hit.gameObject, transform.position);
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
