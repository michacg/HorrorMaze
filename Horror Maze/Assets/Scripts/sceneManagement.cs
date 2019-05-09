using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class sceneManagement : MonoBehaviour
{
    private string scName;
    public static sceneManagement instance;
    public bool sceneFlag = false;
    public sceneMusic[] relations;

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
        sceneFlag = true;   //for background transitions

        scName = scene.name;
        sceneMusic s = Array.Find(relations, x => x.sceneName == scene.name);
        if(s != null && AudioManager.instance.currentSong.name != s.songName)  //allows us to have menu theme consistent during all submenus in main menu
            FindObjectOfType<AudioManager>().Play(s.songName);
    }

    // called when the game is terminated
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}
