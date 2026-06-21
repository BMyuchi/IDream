using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    private static DialogueUI instance;

    private GameObject root;
    private Image portraitImage;
    private TextMeshProUGUI dialogueText;
    private RectTransform optionRoot;
    private Action onClosed;
    private Button[] optionButtons;
    private bool waitingForClose;
    private DialogueStep[] steps;
    private int currentStepIndex;
    private DialogueReply[] currentReplies;
    private Coroutine autoCloseRoutine;
    private const float AutoCloseDelay = 0.45f;

    public static DialogueUI Instance
    {
        get
        {
            if (instance == null)
                instance = CreateInstance();

            return instance;
        }
    }

    public void Show(Sprite portrait, string text, string[] options, Action<int> onOptionSelected, Action closedCallback)
    {
        DialogueStep fallbackStep = new DialogueStep
        {
            npcText = text,
            playerReplies = options
        };

        ShowSequence(portrait, new[] { fallbackStep }, closedCallback);
    }

    public void ShowSequence(Sprite portrait, DialogueStep[] dialogueSteps, Action closedCallback)
    {
        StopAutoClose();
        onClosed = closedCallback;
        steps = dialogueSteps;
        currentStepIndex = 0;
        waitingForClose = false;
        EnsureBuilt();

        root.SetActive(true);
        portraitImage.sprite = portrait;
        portraitImage.enabled = portrait != null;
        ShowCurrentStep();
    }

    void Update()
    {
        if (root == null || !root.activeSelf || Keyboard.current == null)
            return;

        if (waitingForClose)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                Close();

            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            SelectOption(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            SelectOption(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            SelectOption(2);
    }

    void SelectOption(int optionIndex)
    {
        if (waitingForClose || currentReplies == null || optionIndex < 0 || optionIndex >= currentReplies.Length)
            return;

        DialogueReply reply = currentReplies[optionIndex];

        if (reply.endsDialogue)
        {
            ShowEndPrompt();
            return;
        }

        if (reply.nextStepNumber > 0)
            currentStepIndex = reply.nextStepNumber - 1;
        else
            currentStepIndex++;

        if (steps != null && currentStepIndex < steps.Length)
        {
            ShowCurrentStep();
            return;
        }

        ShowEndPrompt();
    }

    public void Close()
    {
        StopAutoClose();

        if (root != null)
            root.SetActive(false);

        Action callback = onClosed;
        onClosed = null;
        steps = null;
        currentReplies = null;
        waitingForClose = false;
        callback?.Invoke();
    }

    void ShowCurrentStep()
    {
        StopAutoClose();
        ClearOptions();

        DialogueStep step = steps != null && currentStepIndex < steps.Length ? steps[currentStepIndex] : null;
        string npcText = step != null ? step.npcText : null;
        currentReplies = step != null ? step.GetReplies() : null;

        dialogueText.text = string.IsNullOrWhiteSpace(npcText) ? "..." : npcText;

        if (currentReplies == null || currentReplies.Length == 0)
            currentReplies = new[] { new DialogueReply { replyText = "继续" } };

        int replyCount = Mathf.Min(currentReplies.Length, 3);
        for (int i = 0; i < replyCount; i++)
        {
            int optionIndex = i;
            Button button = CreateOptionButton((i + 1) + ". " + currentReplies[i].replyText);
            button.onClick.AddListener(() => SelectOption(optionIndex));
        }

        optionButtons = optionRoot.GetComponentsInChildren<Button>();
        Array.Resize(ref currentReplies, replyCount);
        waitingForClose = false;
    }

    void ShowEndPrompt()
    {
        ClearOptions();
        optionButtons = Array.Empty<Button>();
        dialogueText.text = string.Empty;
        waitingForClose = true;
        autoCloseRoutine = StartCoroutine(CloseAfterDelay());
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(AutoCloseDelay);
        autoCloseRoutine = null;
        Close();
    }

    void StopAutoClose()
    {
        if (autoCloseRoutine == null)
            return;

        StopCoroutine(autoCloseRoutine);
        autoCloseRoutine = null;
    }

    private static DialogueUI CreateInstance()
    {
        GameObject uiObject = new GameObject("DialogueUI");
        DontDestroyOnLoad(uiObject);
        DialogueUI ui = uiObject.AddComponent<DialogueUI>();
        ui.EnsureBuilt();
        ui.root.SetActive(false);
        return ui;
    }

    private void EnsureBuilt()
    {
        if (root != null) return;

        Canvas canvas = FindOverlayCanvas();

        root = new GameObject("DialoguePanel");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = new Vector2(0f, 24f);
        rootRect.sizeDelta = new Vector2(760f, 150f);

        Image panelImage = root.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.78f);

        GameObject portraitObject = new GameObject("NpcPortrait");
        portraitObject.transform.SetParent(root.transform, false);
        RectTransform portraitRect = portraitObject.AddComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0f, 0.5f);
        portraitRect.anchorMax = new Vector2(0f, 0.5f);
        portraitRect.pivot = new Vector2(0f, 0.5f);
        portraitRect.anchoredPosition = new Vector2(18f, 0f);
        portraitRect.sizeDelta = new Vector2(96f, 96f);
        portraitImage = portraitObject.AddComponent<Image>();
        portraitImage.preserveAspect = true;

        GameObject textObject = new GameObject("DialogueText");
        textObject.transform.SetParent(root.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(132f, 24f);
        textRect.offsetMax = new Vector2(-230f, -24f);
        dialogueText = textObject.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 22f;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.MidlineLeft;
        dialogueText.textWrappingMode = TextWrappingModes.Normal;

        GameObject optionsObject = new GameObject("Options");
        optionsObject.transform.SetParent(root.transform, false);
        optionRoot = optionsObject.AddComponent<RectTransform>();
        optionRoot.anchorMin = new Vector2(1f, 0.5f);
        optionRoot.anchorMax = new Vector2(1f, 0.5f);
        optionRoot.pivot = new Vector2(1f, 0.5f);
        optionRoot.anchoredPosition = new Vector2(-18f, 0f);
        optionRoot.sizeDelta = new Vector2(200f, 112f);

        VerticalLayoutGroup layout = optionsObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
    }

    private Canvas FindOverlayCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
        }

        GameObject canvasObject = new GameObject("Runtime Dialogue Canvas");
        Canvas newCanvas = canvasObject.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        newCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasObject.AddComponent<GraphicRaycaster>();
        return newCanvas;
    }

    private Button CreateOptionButton(string label)
    {
        GameObject buttonObject = new GameObject("OptionButton");
        buttonObject.transform.SetParent(optionRoot, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200f, 34f);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 34f;
        layoutElement.preferredHeight = 34f;
        layoutElement.minWidth = 200f;
        layoutElement.preferredWidth = 200f;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.92f);

        Button button = buttonObject.AddComponent<Button>();

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 2f);
        labelRect.offsetMax = new Vector2(-8f, -2f);

        TextMeshProUGUI labelText = labelObject.AddComponent<TextMeshProUGUI>();
        labelText.text = string.IsNullOrWhiteSpace(label) ? "继续" : label;
        labelText.fontSize = 18f;
        labelText.color = Color.black;
        labelText.alignment = TextAlignmentOptions.Center;

        return button;
    }

    private void ClearOptions()
    {
        for (int i = optionRoot.childCount - 1; i >= 0; i--)
            Destroy(optionRoot.GetChild(i).gameObject);
    }
}

[Serializable]
public class DialogueStep
{
    [Tooltip("只用于 Inspector 中区分对话段落，不会显示给玩家。")]
    public string stepName;
    [TextArea(2, 4)] public string npcText = "NPC 台词";
    [Tooltip("旧版线性回复。若 Reply Choices 为空，会使用这里。")]
    public string[] playerReplies = { "继续" };
    public DialogueReply[] replyChoices;

    public DialogueReply[] GetReplies()
    {
        if (replyChoices != null && replyChoices.Length > 0)
            return replyChoices;

        if (playerReplies == null || playerReplies.Length == 0)
            return Array.Empty<DialogueReply>();

        DialogueReply[] replies = new DialogueReply[playerReplies.Length];
        for (int i = 0; i < playerReplies.Length; i++)
            replies[i] = new DialogueReply { replyText = playerReplies[i] };

        return replies;
    }
}

[Serializable]
public class DialogueReply
{
    [Tooltip("只用于 Inspector 中区分选项，不会显示给玩家。")]
    public string choiceName;
    public string replyText = "继续";
    [Tooltip("0 表示进入下一段；填 4 表示跳到 Dialogue Steps 的第 4 段。")]
    public int nextStepNumber;
    public bool endsDialogue;
}
