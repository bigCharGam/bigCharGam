using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private GameObject skillLearnUiPrefab;

    private GameObject skillLearnUiInstance;
    private GameObject debugPanel;
    private TMP_InputField gotoXInputField;

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

        debugPanel = CreatePanel(parent, "DebugPanel", new Vector2(0, 1), new Vector2(80, -100), new Vector2(220, 270));
        debugPanel.SetActive(false);

        Button skillUiButton = CreateButton(debugPanel.transform, "ToggleSkillUiButton", "스킬 UI 열기/닫기", new Vector2(0.5f, 1), new Vector2(0, -35), new Vector2(200, 50));
        skillUiButton.onClick.AddListener(ToggleSkillLearnUi);

        Button addExpButton = CreateButton(debugPanel.transform, "AddExpButton", "경험치 +100", new Vector2(0.5f, 1), new Vector2(0, -95), new Vector2(200, 50));
        addExpButton.onClick.AddListener(AddExp100);

        gotoXInputField = CreateInputField(debugPanel.transform, "GotoXInputField", "X좌표", new Vector2(0.5f, 1), new Vector2(0, -155), new Vector2(200, 40));

        Button gotoXButton = CreateButton(debugPanel.transform, "GotoXButton", "X좌표로 이동", new Vector2(0.5f, 1), new Vector2(0, -205), new Vector2(200, 50));
        gotoXButton.onClick.AddListener(GotoXFromInput);
    }

    private void GotoXFromInput()
    {
        if (gotoXInputField == null) return;

        if (int.TryParse(gotoXInputField.text, out int x))
        {
            GotoX(x);
        }
        else
        {
            Debug.LogWarning("[DebugMenu] X좌표에 올바른 정수를 입력하세요.");
        }
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

    public void GotoX(int x)
    {
        if (BattleManager.instance == null)
        {
            Debug.LogWarning("[DebugMenu] BattleManager.instance가 없습니다.");
            return;
        }

        BattleManager.instance.transform.position = new Vector3(x, BattleManager.instance.transform.position.y, BattleManager.instance.transform.position.z);
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

    private TMP_InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 anchor, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject fieldObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        RectTransform rect = fieldObj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = fieldObj.GetComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);

        GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.SetParent(rect, false);
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(8, 4);
        textAreaRect.offsetMax = new Vector2(-8, -4);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.SetParent(textAreaRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 20;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;

        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.SetParent(textAreaRect, false);
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 20;
        placeholderText.fontStyle = FontStyles.Italic;
        placeholderText.color = new Color(0f, 0f, 0f, 0.5f);
        placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
        placeholderText.raycastTarget = false;

        TMP_InputField inputField = fieldObj.GetComponent<TMP_InputField>();
        inputField.textViewport = textAreaRect;
        inputField.textComponent = text;
        inputField.placeholder = placeholderText;
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        return inputField;
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
