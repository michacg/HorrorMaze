using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.GetComponent<Collider>().CompareTag("Player"))
        {
            Debug.Log("entered");
            //Time.timeScale = 0;
        }
    }

}
