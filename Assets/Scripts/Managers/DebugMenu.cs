using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private GameObject skillLearnUiPrefab;

    private GameObject skillLearnUiInstance;
    private GameObject debugPanel;

    private void Awake()
    {
        EnsureEventSystem();
        Canvas canvas = EnsureCanvas();
        BuildUi(canvas.transform);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private Canvas EnsureCanvas()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObj = new GameObject("DebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        return canvas;
    }

    private void BuildUi(Transform parent)
    {
        Button openButton = CreateButton(parent, "OpenDebugButton", "디버그창", new Vector2(0, 1), new Vector2(70, -30), new Vector2(120, 50));
        openButton.onClick.AddListener(ToggleDebugPanel);

        debugPanel = CreatePanel(parent, "DebugPanel", new Vector2(0, 1), new Vector2(80, -100), new Vector2(220, 150));
        debugPanel.SetActive(false);

        Button skillUiButton = CreateButton(debugPanel.transform, "ToggleSkillUiButton", "스킬 UI 열기/닫기", new Vector2(0.5f, 1), new Vector2(0, -35), new Vector2(200, 50));
        skillUiButton.onClick.AddListener(ToggleSkillLearnUi);

        Button addExpButton = CreateButton(debugPanel.transform, "AddExpButton", "경험치 +100", new Vector2(0.5f, 1), new Vector2(0, -95), new Vector2(200, 50));
        addExpButton.onClick.AddListener(AddExp100);
    }

    private void ToggleDebugPanel()
    {
        debugPanel.SetActive(!debugPanel.activeSelf);
    }

    private void ToggleSkillLearnUi()
    {
        if (skillLearnUiInstance == null)
        {
            if (skillLearnUiPrefab == null)
            {
                Debug.LogWarning("[DebugMenu] skillLearnUiPrefab이 지정되지 않았습니다.");
                return;
            }

            skillLearnUiInstance = Instantiate(skillLearnUiPrefab);
            return;
        }

        skillLearnUiInstance.SetActive(!skillLearnUiInstance.activeSelf);
    }

    private void AddExp100()
    {
        if (BattleManager.instance == null)
        {
            Debug.LogWarning("[DebugMenu] BattleManager.instance가 없습니다.");
            return;
        }

        BattleManager.instance.AddExp(100);
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = buttonObj.GetComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        CreateLabel(rect, label);

        return buttonObj.GetComponent<Button>();
    }

    private GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject panelObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = panelObj.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);

        return panelObj;
    }

    private void CreateLabel(Transform parent, string text)
    {
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }
}
