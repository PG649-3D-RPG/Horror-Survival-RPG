using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Note: Needs to be a MonoBehaviour to run coroutines
public class Loader : MonoBehaviour
{

    public void Load(IEnumerator workCoroutine)
    {
        StartCoroutine(LoaderCoroutine(workCoroutine));
    }

    private IEnumerator LoaderCoroutine(IEnumerator workCoroutine)
    {
        SceneManager.LoadScene("Scenes/LoadingScreen/LoadingScreen", LoadSceneMode.Additive);
        SceneManager.LoadScene("Scenes/Level", LoadSceneMode.Additive);
        yield return null;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Level"));
        // Run the actual loading work. Nested coroutines are necessary to that we can wait for the
        // workCoroutine to finish.
        yield return StartCoroutine(workCoroutine);
        SceneManager.UnloadSceneAsync("Scenes/LoadingScreen/LoadingScreen");
    }
}
