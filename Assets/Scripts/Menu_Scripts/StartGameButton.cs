using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    public int sceneIndex; // Scene to load
    public Animator transitionAnimator; // Reference to the Animator on the transition panel

    public void LoadScene()
    {
        StartCoroutine(LoadSceneAfterTransition());
    }

    private IEnumerator LoadSceneAfterTransition()
    {
        // Play the transition animation
        if (transitionAnimator != null)
        {
            transitionAnimator.SetBool("animateOut", true);
        }

        // Wait for the animation to finish (adjust if needed)
        yield return new WaitForSeconds(1f);

        // Load the new scene
        SceneManager.LoadScene(sceneIndex);
    }
}
