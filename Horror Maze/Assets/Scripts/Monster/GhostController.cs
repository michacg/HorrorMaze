using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GhostController : MonoBehaviour
{
    public float ghostSpeed = 2;
    public TrapTrigger trapScript;
    public float lookSpeed = 100;

    private CharacterController controller;
    private GameObject player;
    private Vector3 origin;
    private AudioSource audio1;
    private AudioSource audio2;

    private Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        // Plays monster noises for the duration of the monster's life
        StartMonsterNoise();
        controller = GetComponent<CharacterController>();
        origin = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        player = GameManager.instance.GetPlayerGO();

        Debug.Log(playerCamera.transform.position);

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
            StartCoroutine(JumpScare(hit.gameObject));
        }
    }


    public IEnumerator JumpScare(GameObject player)
    {

        player.GetComponent<MonsterJumpScare>().Show(2);
        player.transform.Find("JumpScareLight").gameObject.SetActive(true);
        player.GetComponent<FirstPersonController>().enabled = false;
        //this.gameObject.SetActive(false);
        GetComponentInChildren<MeshRenderer>().enabled = false;

        Time.timeScale = 0;
        Debug.Log("TIME FREEZE");
        //lock camera movement
        yield return new WaitForSecondsRealtime(1);

        Debug.Log("TIME START AGAIN");
        player.transform.Find("JumpScareLight").gameObject.SetActive(false);
        player.GetComponent<FirstPersonController>().enabled = true;
        Time.timeScale = 1;
        trapScript.Respawn(player, transform.position);
        Destroy(this.gameObject);

        //StartCoroutine(Respawn(player));

    }

    // Starts a monster background noise and then plays a different noise within 30-50 seconds

    private void StartMonsterNoise()
    {
        audio1 = GetComponents<AudioSource>()[0];
        audio2 = GetComponents<AudioSource>()[1];
        audio2.Play();
        StartCoroutine(MonsterNoises());
    }

    private IEnumerator MonsterNoises()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(30, 50));
            Debug.Log("playing scream");
            audio2.Play();
        }
    }
}
