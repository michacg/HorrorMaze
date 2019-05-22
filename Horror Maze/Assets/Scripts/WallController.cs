using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Detecting ghost...");
        if (collision.gameObject.tag.Equals("Ghost"))
        {
            Debug.Log("Ignoring ghost...");
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), collision.gameObject.GetComponent<Collider>());
        }
    }
}
