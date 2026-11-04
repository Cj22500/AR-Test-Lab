using UnityEngine;

public class SceneExperimentConfig : MonoBehaviour
{
    public ExperimentId activeExperiment = ExperimentId.Experiment1;

    [Header("Scene Flow")]
    public string mainMenuSceneName = "MainMenu";

    // Leave blank to auto-map:
    // ExperimentScene -> ExperimentScene2 -> ... -> ExperimentScene5
    public string nextSceneName = "";
}
