using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    public Image screen;
    public float percentage = 0;

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
    }

    public void StartDeath()
    {
        StartCoroutine(FadeScreen());
    }

    public IEnumerator FadeScreen()
    {
        while (true)
        {
            percentage += 0.025f;

            float currentValue = Mathf.Lerp(0, 1, percentage);

            screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, currentValue);

            if (percentage >= 1)
            {
                percentage = 0;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(1.5f);
        screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, 0);
    }
}
