using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public float ghostSpeed = 2;
    public TrapTrigger trapScript;

    private CharacterController controller;
    private GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        // Plays monster noises for the duration of the monster's life
        StartCoroutine(MonsterNoises());

        controller = GetComponent<CharacterController>();

        Debug.Log("Ghost tag = " + gameObject.tag);
    }

    // Update is called once per frame
    void Update()
    {
        // Since monsters are capsules right now, they collide with 
        // walls and fall over. This is to keep them upright.
        transform.eulerAngles = new Vector3(0, 0, 0);
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
