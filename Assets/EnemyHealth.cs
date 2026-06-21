using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public Slider healthBar;
    public ArenaBattleController battle;
    public GameObject visualRoot;

    private int currentHealth;
    private Renderer[] renderers;
    private Collider2D[] colliders;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);
        ResetHealth();
    }

    public void ResetHealth()
    {
        SetEnemyVisible(true);

        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthBar();

        if (currentHealth == 0)
        {
            SetEnemyVisible(false);

            if (battle != null)
                battle.WinBattle();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar == null) return;

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    void SetEnemyVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
            return;
        }

        foreach (Renderer enemyRenderer in renderers)
        {
            if (enemyRenderer != null)
                enemyRenderer.enabled = visible;
        }

        foreach (Collider2D enemyCollider in colliders)
        {
            if (enemyCollider != null)
                enemyCollider.enabled = visible;
        }
    }
}
