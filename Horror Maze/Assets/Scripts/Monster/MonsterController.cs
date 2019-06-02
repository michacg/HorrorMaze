using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MonsterController : MonoBehaviour
{
    public GameObject[] monsters;
    public float delayTime = 10f; // Delay in dead body transition into monster
    public GameObject fireEffect;
    public float fireTime = 3f; // Seconds fire engulfs the dead body

    private Animator animator;

    private void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    public void Transform()
    {
        StartCoroutine(TransitionEffect());
    }

    private IEnumerator TransitionEffect()
    {
        // Wait a few seconds before transition effect kicks in
        yield return new WaitForSeconds(delayTime);

        // Start smoke effects
        Vector3 firePosition = transform.position;
        firePosition.y = -3f;
        GameObject fire = Instantiate(fireEffect, firePosition, Quaternion.identity);
        animator.SetBool("Burning", true);

        yield return new WaitForSeconds(fireTime);
        Destroy(fire);

        int monsterType = 2; // debugging AI purposes
        //int monsterType = Random.Range(0, monsters.Length);
        Vector3 position = transform.position;
        position.y = 1;

        Destroy(gameObject);
        GameObject go = Instantiate(monsters[monsterType], position, Quaternion.identity);
        GameManager.instance.AddMonster(go);
    }
}
