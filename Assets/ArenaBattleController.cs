using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaBattleController : MonoBehaviour
{
    public EnemyHealth enemy;
    public GameObject[] roomDoors;
    public DamageWarningZone damageWarningPrefab;
    public BattleReflectItem reflectItemPrefab;
    public Transform[] warningSpawnPoints;
    public Transform[] itemSpawnPoints;
    public float warningInterval = 1.5f;
    public float itemInterval = 4f;
    public int playerMaxHealth = 3;
    public float playerDamageCooldown = 0.7f;
    public Slider playerHealthBar;
    public bool showPlayerHealthUi = true;
    public Color playerHealthFrameColor = Color.white;
    public Color playerHealthFillColor = new Color(0.1f, 0.45f, 1f, 1f);
    public NarrativeNPC npcAfterBossDefeat;
    public Vector3 npcAfterBossDefeatOffset = new Vector3(1.5f, 0f, 0f);

    private playerController currentPlayer;
    private Coroutine warningRoutine;
    private Coroutine itemRoutine;
    private bool isBattleActive;
    private bool isBattleCompleted;
    private int currentPlayerHealth;
    private bool canDamagePlayer = true;
    private RectTransform playerHealthUiRoot;
    private Image playerHealthFillImage;
    private TextMeshProUGUI playerHealthText;
    private readonly List<DamageWarningZone> spawnedWarnings = new List<DamageWarningZone>();
    private readonly List<BattleReflectItem> spawnedItems = new List<BattleReflectItem>();

    public bool IsBattleActive => isBattleActive;
    public bool IsBattleCompleted => isBattleCompleted;

    public void StartBattle(playerController player)
    {
        if (isBattleActive || isBattleCompleted) return;

        Debug.Log("Arena battle started.", this);

        currentPlayer = player;
        isBattleActive = true;
        currentPlayerHealth = playerMaxHealth;
        canDamagePlayer = true;
        EnsurePlayerHealthUi();
        SetPlayerHealthUiVisible(true);
        UpdatePlayerHealthBar();

        SetDoorsClosed(true);

        if (enemy != null)
        {
            enemy.battle = this;
            enemy.ResetHealth();
        }
        else
        {
            Debug.LogWarning("ArenaBattleController has no enemy assigned.", this);
        }

        if (roomDoors == null || roomDoors.Length == 0)
            Debug.LogWarning("ArenaBattleController has no roomDoors assigned, so no doors will close.", this);

        if (damageWarningPrefab == null)
            Debug.LogWarning("ArenaBattleController has no damageWarningPrefab assigned.", this);

        if (warningSpawnPoints == null || warningSpawnPoints.Length == 0)
            Debug.LogWarning("ArenaBattleController has no warningSpawnPoints assigned.", this);

        warningRoutine = StartCoroutine(SpawnWarnings());
        itemRoutine = StartCoroutine(SpawnItems());
    }

    public void WinBattle()
    {
        if (!isBattleActive) return;

        StopBattleLoops();
        isBattleActive = false;
        isBattleCompleted = true;
        SetDoorsClosed(false);
        ClearSpawnedBattleObjects();
        SetPlayerHealthUiVisible(false);

        if (npcAfterBossDefeat != null && currentPlayer != null)
            npcAfterBossDefeat.AppearNear(currentPlayer, npcAfterBossDefeatOffset);
    }

    public void PlayerDied()
    {
        if (!isBattleActive) return;

        StopBattleLoops();
        isBattleActive = false;
        SetDoorsClosed(false);
        ClearSpawnedBattleObjects();
        SetPlayerHealthUiVisible(false);

        if (enemy != null)
            enemy.ResetHealth();
    }

    IEnumerator SpawnWarnings()
    {
        while (isBattleActive)
        {
            SpawnWarning();
            yield return new WaitForSeconds(warningInterval);
        }
    }

    IEnumerator SpawnItems()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(itemInterval);
            SpawnReflectItem();
        }
    }

    void SpawnWarning()
    {
        if (damageWarningPrefab == null || warningSpawnPoints == null || warningSpawnPoints.Length == 0)
            return;

        Transform spawnPoint = warningSpawnPoints[Random.Range(0, warningSpawnPoints.Length)];
        DamageWarningZone warning = Instantiate(damageWarningPrefab, spawnPoint.position, spawnPoint.rotation);
        warning.Initialize(this);
        spawnedWarnings.Add(warning);
    }

    void SpawnReflectItem()
    {
        if (reflectItemPrefab == null || itemSpawnPoints == null || itemSpawnPoints.Length == 0 || enemy == null)
            return;

        Transform spawnPoint = itemSpawnPoints[Random.Range(0, itemSpawnPoints.Length)];
        BattleReflectItem item = Instantiate(reflectItemPrefab, spawnPoint.position, spawnPoint.rotation);
        item.Initialize(enemy, this);
        spawnedItems.Add(item);
    }

    void SetDoorsClosed(bool closed)
    {
        if (roomDoors == null) return;

        foreach (GameObject door in roomDoors)
        {
            if (door != null)
                door.SetActive(closed);
        }
    }

    void StopBattleLoops()
    {
        if (warningRoutine != null)
        {
            StopCoroutine(warningRoutine);
            warningRoutine = null;
        }

        if (itemRoutine != null)
        {
            StopCoroutine(itemRoutine);
            itemRoutine = null;
        }
    }

    void ClearSpawnedBattleObjects()
    {
        for (int i = spawnedWarnings.Count - 1; i >= 0; i--)
        {
            if (spawnedWarnings[i] != null)
                Destroy(spawnedWarnings[i].gameObject);
        }

        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            if (spawnedItems[i] != null)
                Destroy(spawnedItems[i].gameObject);
        }

        spawnedWarnings.Clear();
        spawnedItems.Clear();
    }

    public void UnregisterWarning(DamageWarningZone warning)
    {
        spawnedWarnings.Remove(warning);
    }

    public void UnregisterReflectItem(BattleReflectItem item)
    {
        spawnedItems.Remove(item);
    }

    public void DamagePlayer(playerController player)
    {
        if (!isBattleActive || player == null || player != currentPlayer || !canDamagePlayer)
            return;

        currentPlayerHealth = Mathf.Max(0, currentPlayerHealth - 1);
        UpdatePlayerHealthBar();

        if (currentPlayerHealth == 0)
        {
            player.Respawn();
            PlayerDied();
            return;
        }

        StartCoroutine(PlayerDamageCooldown());
    }

    IEnumerator PlayerDamageCooldown()
    {
        canDamagePlayer = false;
        yield return new WaitForSeconds(playerDamageCooldown);
        canDamagePlayer = true;
    }

    void UpdatePlayerHealthBar()
    {
        float healthPercent = playerMaxHealth <= 0 ? 0f : (float)currentPlayerHealth / playerMaxHealth;

        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = playerMaxHealth;
            playerHealthBar.value = currentPlayerHealth;
        }

        if (playerHealthFillImage != null)
            playerHealthFillImage.fillAmount = healthPercent;

        if (playerHealthText != null)
            playerHealthText.text = currentPlayerHealth + "/" + playerMaxHealth;
    }

    void EnsurePlayerHealthUi()
    {
        if (!showPlayerHealthUi || playerHealthUiRoot != null) return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Runtime UI Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject rootObject = new GameObject("PlayerHealthUI");
        rootObject.transform.SetParent(canvas.transform, false);
        playerHealthUiRoot = rootObject.AddComponent<RectTransform>();
        playerHealthUiRoot.anchorMin = new Vector2(0f, 1f);
        playerHealthUiRoot.anchorMax = new Vector2(0f, 1f);
        playerHealthUiRoot.pivot = new Vector2(0f, 1f);
        playerHealthUiRoot.anchoredPosition = new Vector2(20f, -20f);
        playerHealthUiRoot.sizeDelta = new Vector2(250f, 30f);

        GameObject frameObject = new GameObject("HealthFrame");
        frameObject.transform.SetParent(rootObject.transform, false);
        RectTransform frameRect = frameObject.AddComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0f, 0.5f);
        frameRect.anchorMax = new Vector2(0f, 0.5f);
        frameRect.pivot = new Vector2(0f, 0.5f);
        frameRect.anchoredPosition = Vector2.zero;
        frameRect.sizeDelta = new Vector2(160f, 24f);
        Image frameImage = frameObject.AddComponent<Image>();
        frameImage.color = playerHealthFrameColor;

        GameObject fillObject = new GameObject("HealthFill");
        fillObject.transform.SetParent(frameObject.transform, false);
        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        playerHealthFillImage = fillObject.AddComponent<Image>();
        playerHealthFillImage.color = playerHealthFillColor;
        playerHealthFillImage.type = Image.Type.Filled;
        playerHealthFillImage.fillMethod = Image.FillMethod.Horizontal;
        playerHealthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        GameObject textObject = new GameObject("HealthText");
        textObject.transform.SetParent(rootObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.5f);
        textRect.anchorMax = new Vector2(0f, 0.5f);
        textRect.pivot = new Vector2(0f, 0.5f);
        textRect.anchoredPosition = new Vector2(172f, 0f);
        textRect.sizeDelta = new Vector2(70f, 24f);
        playerHealthText = textObject.AddComponent<TextMeshProUGUI>();
        playerHealthText.text = playerMaxHealth + "/" + playerMaxHealth;
        playerHealthText.fontSize = 18f;
        playerHealthText.color = Color.white;
        playerHealthText.alignment = TextAlignmentOptions.MidlineLeft;
    }

    void SetPlayerHealthUiVisible(bool visible)
    {
        if (playerHealthUiRoot != null)
            playerHealthUiRoot.gameObject.SetActive(visible);
    }
}
