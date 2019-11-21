using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{ 
    public float raycastGhostAngle = 5;
    public float raycastDollAngle = 20;
    public float raycastDistance = 1;
    public LayerMask layerMask;

    private Camera fpsCamera;
    private List<GameObject> monsters = new List<GameObject>();
    private List<DollController> dolls = new List<DollController>();

    // Start is called before the first frame update
    void Start()
    {
        fpsCamera = GetComponentInParent<Camera>();
    }

    private List<GameObject> MonsterInSight()
    {
        List<GameObject> result = new List<GameObject>();

        foreach (GameObject go in monsters)
        {
            if (go == null)
            {
                continue;
            }
            Vector3 direction = go.transform.position - fpsCamera.transform.position;

            // If the monster is within the cone detection range
            // (i.e. within both angle and distance range) then 
            // preliminary check is passed. 
            float raycastAngle = raycastGhostAngle;
            if (go.tag.Equals("Doll"))
            {
                raycastAngle = raycastDollAngle;
            }
            if ((Vector3.Angle(direction, fpsCamera.transform.forward) <= raycastAngle) &&
                (Vector3.Distance(go.transform.position, fpsCamera.transform.position) <= raycastDistance))
            {
                result.Add(go);
            }
        }

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        monsters = GameManager.instance.GetAllMonsters();

        List<GameObject> monstersInRange = MonsterInSight();
        if (monstersInRange.Count != 0)
        {
            SendRaycast(monstersInRange);
        }
        else
        {
            // Make all dolls that are not seen moveable again. 
            foreach (DollController dollScript in dolls)
            {
                dollScript.CanMove(true);
            }
            dolls.Clear();
        }
    }

    private void SendRaycast(List<GameObject> monstersInRange)
    {
        foreach (GameObject go in monstersInRange)
        {
            // Translate camera's center position to worldpoint position.
            Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

            Vector3 rayDirection = (go.transform.position - fpsCamera.transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(fpsCamera.transform.position, rayDirection, out hit, raycastDistance, layerMask))
            {
                Debug.DrawRay(transform.position, rayDirection * hit.distance, Color.yellow);

                GameObject monsterHit = hit.transform.gameObject;
                if (monsterHit.tag.Equals("Ghost"))
                {
                    GhostController ghostScript = monsterHit.GetComponent<GhostController>();
                    ghostScript.Restart();
                }
                else if (monsterHit.tag.Equals("Doll"))
                {
                    DollController dollScript = monsterHit.GetComponent<DollController>();
                    dollScript.CanMove(false);
                    dolls.Add(dollScript);
                }
            }
            else
            {
                Debug.DrawRay(fpsCamera.transform.position, rayDirection * raycastDistance, Color.white);

                // if the GameObject is a doll, it is in the range
                // of player's detection, however it is behind a wall
                // or player is not looking at it. Should make the doll
                // moveable again. 
                if (go.tag.Equals("Doll"))
                {
                    go.GetComponent<DollController>().CanMove(true);
                }
            }
        }
    }
}
