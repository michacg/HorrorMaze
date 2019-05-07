using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour
{
    public GameObject GM;
    public Generator GMScript;

    void Start()
    {
        GM = GameObject.FindGameObjectWithTag("GameManager");
        GMScript = GM.GetComponent<Generator>();
    }
    private void OnTriggerEnter(Collider col)
    {

        if(col.GetComponent<Collider>().CompareTag("Player"))
        {
            //Time.timeScale = 0;
            //GameEnded(this);
            //GameManager GMScript = GM.GetComponent<GameManager>();
            //GameObject.FindGameObjectWithTag("GameManager").GetComponent<Generator>().endGamed();
            //Debug.Log(GM.gameOver);
            //GM.endGame();
            GMScript.endGame();
        }
    }

}
