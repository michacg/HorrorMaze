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
    sceneMusic s = null;
    private List<sceneMusic> soundList = new List<sceneMusic>();
    private IEnumerator playRandom;

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
        playRandom = FindObjectOfType<AudioManager>().PlayRandom();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "SampleScene")    //specific to this project
        {
            StartCoroutine(playRandom);

        }
        else
        {
            StopCoroutine(playRandom);
        }
        sceneFlag = true;   //for background transitions
        if(s != null && s.songName != null)
        {
            while(s != null)  //allows multiple different songs to play upon loading a sccene at the same time
            {
                s = Array.Find(relations, x => x.sceneName == scene.name && !soundList.Contains(x));
                if(s != null && soundList.Count == 0)    //this is the case where the first song fades out the last scene's song and fades in
                {
                    soundList.Add(s);
                    FindObjectOfType<AudioManager>().DialogueTransitionSong(s.songName);
                }
                else if(s != null && soundList.Count != 0)      //this is the case where the first song has already faded out the last scenes song, so this just 
                {                                               //fades in another song
                    soundList.Add(s);
                    FindObjectOfType<AudioManager>().Play(s.songName);
                }
            }
        }
        //mostly for the opening scene as well as any scene that doesnt have a relation to continue the theme going
        else
        {
            s = Array.Find(relations, x => x.sceneName == scene.name);
            if(s != null && AudioManager.instance.currentSong.name != s.songName) //allows us to have menu theme consistent during all submenus in main menu
            {
                FindObjectOfType<AudioManager>().Play(s.songName);  
            }
        }
    }

    // called when the game is terminated
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}
