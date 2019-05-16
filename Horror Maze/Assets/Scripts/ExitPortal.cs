using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour
{
    public GameObject GM;
    public Generator GMScript;

    private void OnTriggerEnter(Collider col)
    {

        if(col.GetComponent<Collider>().CompareTag("Player"))
        {
            // GameManager.instance.endGame();
            SceneManager.LoadScene("GameOverScene");
        }
    }

}
