using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    public Image deathScreen;
    public Image flashScreen;
    public TextMeshProUGUI storyText;
    public TextAsset storyFile;

    private string[] storyLines;

    public float percentage = 0;
    public bool inDeath = false;
    public int deathCount = 0;

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

        storyLines = storyFile.text.Split('\n');
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

            float currentValue = Mathf.Lerp(0, 0.75f, percentage); //middle value is maximum opacity

            deathScreen.color = new Color(deathScreen.color.r, deathScreen.color.g, deathScreen.color.b, currentValue);

            if (deathCount < 20)
            {
                storyText.text = storyLines[deathCount]; //lines 1-20 get shown in order
            }
            else
            {
                storyText.text = "";
            }
            
            storyText.color = new Color(storyText.color.r, storyText.color.g, storyText.color.b, currentValue);

            if (percentage >= 1)
            {
                percentage = 0;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(2.25f);
        deathScreen.color = new Color(deathScreen.color.r, deathScreen.color.g, deathScreen.color.b, 0);
        storyText.color = new Color(storyText.color.r, storyText.color.g, storyText.color.b, 0);
        inDeath = false;
        deathCount++;
    }
}
