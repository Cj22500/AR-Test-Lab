using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSuccessOverlay : MonoBehaviour
{
    [SerializeField] private GameObject root;   // your panel/root
    [SerializeField] private TMP_Text infoText; // middle textbox

    private string _mainMenuScene = "MainMenu";
    private string _nextScene = "";

    private void Awake() => Hide();

    // Called by ExperimentManager after reading SceneExperimentConfig
    public void Configure(string mainMenuSceneName, string nextSceneName)
    {
        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
            _mainMenuScene = mainMenuSceneName;

        _nextScene = nextSceneName ?? "";
    }

    public void Show(string message)
    {
        if (infoText) infoText.text = message;
        if (root) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
        else gameObject.SetActive(false);
    }

    // Hook this to your "Next Level" button
    public void OnNextLevel()
    {
        Hide();

        if (!string.IsNullOrWhiteSpace(_nextScene))
        {
            SceneManager.LoadScene(_nextScene);
            return;
        }

        // fallback: next build index
        int idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx + 1);
    }

    // Hook this to your "Main Menu" button
    public void OnMainMenu()
    {
        Hide();
        SceneManager.LoadScene(_mainMenuScene);
    }
}
