using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // These names must match your Scene names exactly
    public void LoadExplore()
    {
        SceneManager.LoadScene("ExploreScene"); 
    }

    public void LoadExperiment()
    {
        SceneManager.LoadScene("ExperimentScene");
    }
}
