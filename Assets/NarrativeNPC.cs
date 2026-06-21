using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NarrativeNPC : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite portrait;
    public string promptText = "对话按Q";
    [TextArea(2, 4)] public string dialogueText = "这里之后会填入正式剧情对白。";
    [Header("Player Replies")]
    public string[] options = { "继续" };
    [Header("Multi Step Dialogue")]
    public DialogueStep[] dialogueSteps;
    public bool disappearAfterConversation = true;
    public float fadeDuration = 0.45f;

    private TextMeshPro promptLabel;
    private Collider2D triggerCollider;
    private playerController nearbyPlayer;
    private bool isDialogueOpen;
    private bool isVisible = true;
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = CreateTrianglePlaceholder();

        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider == null)
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();

        triggerCollider.isTrigger = true;
        CreatePromptLabel();
        SetPromptVisible(false);
    }

    void Update()
    {
        if (!isVisible || isDialogueOpen || nearbyPlayer == null || Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            StartDialogue();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        playerController player = other.GetComponentInParent<playerController>();
        if (player == null) return;

        nearbyPlayer = player;
        SetPromptVisible(isVisible && !isDialogueOpen);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        playerController player = other.GetComponentInParent<playerController>();
        if (player == null || player != nearbyPlayer) return;

        nearbyPlayer = null;
        SetPromptVisible(false);
    }

    public void AppearNear(playerController player, Vector3 offset)
    {
        if (player != null)
            transform.position = player.transform.position + offset;

        gameObject.SetActive(true);
        isVisible = true;
        SetAlpha(0f);
        StartFade(1f);
    }

    public void HideImmediate()
    {
        isVisible = false;
        SetPromptVisible(false);
        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    void StartDialogue()
    {
        isDialogueOpen = true;
        SetPromptVisible(false);

        if (nearbyPlayer != null)
            nearbyPlayer.SetControlsLocked(true);

        if (dialogueSteps != null && dialogueSteps.Length > 0)
            DialogueUI.Instance.ShowSequence(portrait, dialogueSteps, EndDialogue);
        else
            DialogueUI.Instance.Show(portrait, dialogueText, options, null, EndDialogue);
    }

    void EndDialogue()
    {
        isDialogueOpen = false;

        if (nearbyPlayer != null)
            nearbyPlayer.SetControlsLocked(false);

        if (disappearAfterConversation)
            StartFade(0f);
        else
            SetPromptVisible(nearbyPlayer != null);
    }

    void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTo(targetAlpha));
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = spriteRenderer != null ? spriteRenderer.color.a : 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
        fadeRoutine = null;

        if (targetAlpha <= 0f)
        {
            isVisible = false;
            SetPromptVisible(false);
            gameObject.SetActive(false);
        }
    }

    void CreatePromptLabel()
    {
        GameObject labelObject = new GameObject("TalkPrompt");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        promptLabel = labelObject.AddComponent<TextMeshPro>();
        promptLabel.text = promptText;
        promptLabel.fontSize = 3f;
        promptLabel.alignment = TextAlignmentOptions.Center;
        promptLabel.color = Color.white;
        promptLabel.sortingOrder = 20;
    }

    void SetPromptVisible(bool visible)
    {
        if (promptLabel != null)
            promptLabel.gameObject.SetActive(visible);
    }

    void SetAlpha(float alpha)
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    SpriteRenderer CreateTrianglePlaceholder()
    {
        GameObject visual = new GameObject("TrianglePlaceholder");
        visual.transform.SetParent(transform, false);
        visual.transform.localScale = new Vector3(0.55f, 1.65f, 1f);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateTriangleSprite();
        renderer.color = Color.white;
        renderer.sortingOrder = 5;
        return renderer;
    }

    Sprite CreateTriangleSprite()
    {
        const int width = 32;
        const int height = 64;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color fill = Color.white;

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            float halfWidth = Mathf.Lerp(width * 0.42f, 1f, t);
            float center = (width - 1) * 0.5f;

            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, Mathf.Abs(x - center) <= halfWidth ? fill : clear);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 32f);
    }
}
