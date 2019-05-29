using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class TablePortal : MonoBehaviour
{
    public GameObject GM;
    public Generator GMScript;
    public FadeTransition fade;

    private void OnTriggerEnter(Collider col)
    {

        if (col.GetComponent<Collider>().CompareTag("Player"))
        {
            Debug.Log("triggered");
            fade.FadeToScene("Start_Menu");
        }
    }

}