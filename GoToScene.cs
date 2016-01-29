using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour {

    public bool fireOnAwake;
    public float delay;
    public bool additive;
    [Header("leave blank for next scene")]
    public string sceneName;

    bool fired;

    public void Fire()
    {
        if (!fired)
        {
            fired = true;
            StartCoroutine(LoadScene());
        }
        else
        {
            Debug.Log("go to scene has already fired once");
        }
    }

    IEnumerator LoadScene()
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        if (sceneName != "")
        {
            //load scene
            if (additive)
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }
        else
        {
            //load next scene
            if (additive)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1, LoadSceneMode.Single);
            }
        }

    }


}
