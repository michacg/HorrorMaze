using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Linq;


public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public Sound[] effects;
    public static AudioManager instance = null;
    private IEnumerator fadeIn;
    private IEnumerator fadeOut;
    [HideInInspector]
    public bool CR_running = false;
    [HideInInspector]
    public List<string> currentSongs = new List<string>();
    [HideInInspector]
    public Sound currentEffect = null;

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
        foreach(Sound track in sounds)  //sets all initial values of audio source to be whats in inspector when those sounds are played
        {
            track.source = gameObject.AddComponent<AudioSource>();
            track.source.clip = track.clip;
            track.source.volume = track.volume; //sets the initial volume to what is in inspector
            track.source.pitch = track.pitch;
            track.source.loop = track.loop;
        }
        foreach(Sound track in effects)  //sets all initial values of audio source to be whats in inspector when those sounds are played
        {
            track.source = gameObject.AddComponent<AudioSource>();
            track.source.clip = track.clip;
            track.source.volume = track.volume; //sets the initial volume to what is in inspector
            track.source.pitch = track.pitch;
            track.source.loop = track.loop;
        }
    }

    public void Play (string name)  //s.source.volume will adjust actual volume. s.volume will adjust initial value which has no meaning here
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        currentSongs.Add(s.name);
        //s.source.volume = 0f;   //makes sure we always start at zero if last coroutine didnt finish. Take out this line if not necessary
        fadeIn = FadeIn(s);     //we assign coroutines only when we start the song. These same references are used when we stop the song
        fadeOut = FadeOut(s);   //this is used to hard stop the last coroutine in case the player spam clicks
        if(s == null)
        {
            Debug.Log("ERROR: Sound not found");
            return;
        }
        StopCoroutine(fadeOut);    //ensures that fading out does not occur simultaneously to fading in
        StartCoroutine(fadeIn);

    }

    public void PlayImmediate (string name)  //s.source.volume will adjust actual volume. s.volume will adjust initial value which has no meaning here
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        currentSongs.Add(s.name);
        //s.source.volume = 0f;   //makes sure we always start at zero if last coroutine didnt finish. Take out this line if not necessary
        if(s == null)
        {
            Debug.Log("ERROR: Sound not found");
            return;
        }
        s.source.Play();

    }


    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        currentSongs.Remove(s.name);
        if(s == null)
        {
            Debug.Log("ERROR: Sound not found to stop");
            return;
        }
        StopCoroutine(fadeIn);  
        if(s.source.isPlaying)
            StartCoroutine(fadeOut);
    }


    public void PlayEffect (string name)  //s.source.volume will adjust actual volume. s.volume will adjust initial value which has no meaning here
    {
        Sound s = Array.Find(effects, sound => sound.name == name);
        currentEffect = s;
        //s.source.volume = 0f;   //makes sure we always start at zero if last coroutine didnt finish. Take out this line if not necessary
        if(s == null)
        {
            Debug.Log("ERROR: Sound not found");
            return;
        }
        s.source.Play();
    }

    public void StopEffect(string name)
    {
        Sound s = Array.Find(effects, sound => sound.name == name);
        currentEffect = null;
        if(s == null)
        {
            Debug.Log("ERROR: Sound not found to stop");
            return;
        }
        s.source.Stop();
    }


    public IEnumerator FadeOut(Sound s)
    {
        CR_running = true;
        while(s.source.volume > 0.01f)
        {
            s.source.volume -= Time.deltaTime / s.fadeOutTime;  //For a duration of fadeTime, volume gradually decreases till its 0
            yield return null;
        }
        s.source.volume = 0f;
        s.source.Stop();
        CR_running = false;
    }

    public IEnumerator FadeIn(Sound s)
    {
        CR_running = true;
        s.source.Play();
        while (s.source.volume < 1.0f)
        {
            s.source.volume += Time.deltaTime / s.fadeInTime; //fades in over course of seconds fadeTime
            yield return null;
        }
        CR_running = false;
    }    

    public void DialogueTransitionSong(List<string> removeSongs, List<string> playSongs)
    {
        foreach(string x in removeSongs)
        {
            Stop(x);
            StartCoroutine(WaitForAudioFade());
        }
        foreach(string y in playSongs)
        {
            Play(y);
            StartCoroutine(WaitForAudioFade());
        }
    }

    public IEnumerator PlayRandom()
    {
        int rand;
        int randInterval;
        int minutes = 0;
        int randMax = 76;           //starts at a 3/51 chance to play a sound but later the frequency increases as the game goes on 
        while (true)
        {
            yield return new WaitForSeconds(5);
            randInterval = UnityEngine.Random.Range(0, randMax);
            if(randInterval > 3)
            {
                if(randMax >= 11)
                {
                    randMax = randMax - 1;
                }
            }
            else
            {
                rand = UnityEngine.Random.Range(0, effects.Length);
                PlayEffect(effects[rand].name);
                minutes++;
                yield return new WaitForSeconds(25);
            }
        }

    }

    public IEnumerator WaitForAudioFade()   //this function will force the fade to only run when the last fade has completed
    {
        while(CR_running)
        {
            yield return null;
        }
    }
}
