using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private List<GameObject> ghosts;
    private Collider collide;
    private int numGhosts = 0;

    private void Start()
    {
        collide = gameObject.GetComponent<Collider>();
    }

    private void Update()
    {
        ghosts = GameManager.instance.FindGhosts();
        if (numGhosts != ghosts.Count)
        {
            foreach (GameObject go in ghosts)
            {
                Physics.IgnoreCollision(collide, go.GetComponent<Collider>());
            }

            numGhosts = ghosts.Count;
        }
    }
}
