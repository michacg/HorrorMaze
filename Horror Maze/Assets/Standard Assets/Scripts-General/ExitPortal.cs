using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if(col.GetComponent<Collider>().CompareTag("Player"))
        {
            Debug.Log("GAME IS OVER");
            Time.timeScale = 0;
            //GameEnded(this);
            //GameManager GMScript = GM.GetComponent<GameManager>();
            //FindObjectOfType<GameManager>().endGame();
            //Debug.Log(GM.gameOver);
            //GM.endGame();
        }
    }

}
