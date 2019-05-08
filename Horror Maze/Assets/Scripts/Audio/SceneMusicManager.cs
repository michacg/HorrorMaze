using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneMusicManager : MonoBehaviour
{
    private string scName;
    public static SceneMusicManager instance = null;
    public bool sceneFlag = false;

    void Awake()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneFlag = true;
        scName = scene.name.Split(' ')[0];
        foreach(Sound x in AudioManager.instance.sounds)
        {
            if(x.name == scName)
            {
                FindObjectOfType<AudioManager>().Play(scName);
            }
        }

    }

    // called when the game is terminated
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}
