using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = player.transform.position + new Vector3(0, 0, 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            // Move respawn code from FirstPersonController here. 
            Debug.Log("Replacing current body with monster body");

            Debug.Log("Respawning player in a new location");
        }
    }
}
