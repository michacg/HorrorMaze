using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    public Image deathScreen;
    public Image flashScreen;
    public float percentage = 0;
    public bool inDeath = false;
    private string[] storyLines;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(instance);

        //BuildStoryArray(Resources.Load<TextAsset>("Story.txt"));
    }

    public void StartDeath()
    {
        StartCoroutine(FadeScreen());
    }

    public IEnumerator FadeScreen()
    {
        inDeath = true;
        bool hideWhite = false;

        while (true) //white flash upon stepping on a trap
        {
            if (!hideWhite)
            {
                percentage += 0.125f;
                float currentValue = Mathf.Lerp(0, 1, percentage);
                flashScreen.color = new Color(flashScreen.color.r, flashScreen.color.g, flashScreen.color.b, currentValue);

                if (percentage >= 1)
                {
                    percentage = 1;
                    hideWhite = true;
                }
            }
            else
            {
                percentage -= 0.125f;
                float currentValue = Mathf.Lerp(0, 1, percentage);
                flashScreen.color = new Color(flashScreen.color.r, flashScreen.color.g, flashScreen.color.b, currentValue);

                if (percentage <= 0)
                {
                    percentage = 0;
                    hideWhite = false;
                    break;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        while (true) //black fade after the white flash
        {
            percentage += 0.025f;

            float currentValue = Mathf.Lerp(0, 1, percentage);

            deathScreen.color = new Color(deathScreen.color.r, deathScreen.color.g, deathScreen.color.b, currentValue);

            if (percentage >= 1)
            {
                percentage = 0;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(1.5f);
        deathScreen.color = new Color(deathScreen.color.r, deathScreen.color.g, deathScreen.color.b, 0);
        inDeath = false;
    }

    public void BuildStoryArray(TextAsset textFile)
    {
        storyLines = textFile.text.Split('\n');
        Debug.Log(storyLines.Length);
    }
}
