using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public Animator animator;

    private string sceneToLoad;
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeToScene (string sceneName)
    {
        sceneToLoad = sceneName;
        animator.SetTrigger("FadeOut");
    }

    public void OnFadeComplete ()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}

