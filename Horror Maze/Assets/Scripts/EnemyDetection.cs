using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    public float raycastDistance = 1;

    private Camera fpsCamera;

    // Start is called before the first frame update
    void Start()
    {
        fpsCamera = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        SendRaycast();
    }

    private void SendRaycast()
    {
        // Translate camera's center position to worldpoint position.
        Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

        // Bit shift the index to layer 12, which is the monster layer.
        // So that Physics.Raycast only detects monsters.
        int layerMask = 1 << 12;

        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, raycastDistance, layerMask))
        {
            Debug.DrawRay(transform.position, fpsCamera.transform.forward * hit.distance, Color.yellow);
            Debug.Log("Did Hit");

            GameObject monsterHit = hit.transform.gameObject;
            if (monsterHit.tag.Equals("Ghost"))
            {
                GhostController ghostScript = monsterHit.GetComponent<GhostController>();
                ghostScript.Restart();
            }
        }
        else
        {
            Debug.DrawRay(transform.position, fpsCamera.transform.forward * raycastDistance, Color.white);
            Debug.Log("Did not Hit");
        }
    }
}
