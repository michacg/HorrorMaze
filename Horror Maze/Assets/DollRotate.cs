using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollRotate : MonoBehaviour
{
    float rotationsPerMinute = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, (float)(6.0 * rotationsPerMinute * Time.deltaTime));
    }
}
