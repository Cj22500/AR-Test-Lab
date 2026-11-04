using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionSuccessOverlay_AutoUI : MonoBehaviour
{
    private GameObject _root;
    private TMP_Text _infoText;

    private string _mainMenuScene = "MainMenu";
    private string _nextScene = "";

    private void Awake()
    {
        BuildUI();
        Hide();
    }

    public void Configure(string mainMenuSceneName, string nextSceneName)
    {
        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
            _mainMenuScene = mainMenuSceneName;

        _nextScene = nextSceneName ?? "";
    }

    public void Show(string message)
    {
        if (_infoText) _infoText.text = message;
        _root.SetActive(true);
    }

    public void Hide()
    {
        if (_root) _root.SetActive(false);
    }

    private void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("MissionSuccessCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // Root overlay
        _root = new GameObject("OverlayRoot", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(canvasGO.transform, false);

        var rootRT = _root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        var rootImg = _root.GetComponent<Image>();
        rootImg.color = new Color(0f, 0f, 0f, 0.65f);

        // Card
        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        card.transform.SetParent(_root.transform, false);

        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(900, 900);
        cardRT.anchoredPosition = Vector2.zero;

        card.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);

        var vlg = card.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(40, 40, 40, 40);
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Title
        var title = CreateTMPText("TitleText", "Mission Success!", 48);
        title.transform.SetParent(card.transform, false);
        title.alignment = TextAlignmentOptions.Center;

        // Info
        _infoText = CreateTMPText("InfoText", "Additional info here...", 30);
        _infoText.transform.SetParent(card.transform, false);
        _infoText.alignment = TextAlignmentOptions.Center;
        _infoText.enableWordWrapping = true;

        var infoLE = _infoText.gameObject.AddComponent<LayoutElement>();
        infoLE.preferredHeight = 420;

        // Buttons
        var nextBtn = CreateButton("NextLevelButton", "Next Level");
        nextBtn.transform.SetParent(card.transform, false);
        nextBtn.onClick.AddListener(OnNextLevel);

        var menuBtn = CreateButton("MainMenuButton", "Main Menu");
        menuBtn.transform.SetParent(card.transform, false);
        menuBtn.onClick.AddListener(OnMainMenu);
    }

    private TMP_Text CreateTMPText(string name, string text, float size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = Color.black;
        return tmp;
    }

    private Button CreateButton(string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.55f, 0.95f, 1f);

        var btn = go.GetComponent<Button>();

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 110);

        var text = CreateTMPText("Text", label, 34);
        text.transform.SetParent(go.transform, false);
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        var textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btn;
    }

    public void OnNextLevel()
    {
        Hide();

        if (!string.IsNullOrWhiteSpace(_nextScene))
        {
            SceneManager.LoadScene(_nextScene);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OnMainMenu()
    {
        Hide();
        SceneManager.LoadScene(_mainMenuScene);
    }
}
