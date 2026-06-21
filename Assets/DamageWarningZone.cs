using System.Collections;
using UnityEngine;

public class DamageWarningZone : MonoBehaviour
{
    public float warningTime = 1f;
    public float activeTime = 0.35f;
    public Collider2D damageCollider;
    public SpriteRenderer warningVisual;
    public SpriteRenderer damageVisual;
    public Color warningColor = new Color(1f, 0.85f, 0f, 0.45f);
    public Color damageColor = new Color(1f, 0f, 0f, 0.85f);

    private ArenaBattleController battle;
    private bool isActive;

    public void Initialize(ArenaBattleController owner)
    {
        battle = owner;
    }

    void Awake()
    {
        if (damageCollider == null)
            damageCollider = GetComponent<Collider2D>();

        if (warningVisual == null)
            warningVisual = GetComponent<SpriteRenderer>();

        if (warningVisual == null)
            warningVisual = GetComponentInChildren<SpriteRenderer>();

        if (damageCollider != null)
            damageCollider.isTrigger = true;

        SetDamageActive(false);
    }

    void Start()
    {
        StartCoroutine(WarningThenDamage());
    }

    IEnumerator WarningThenDamage()
    {
        yield return new WaitForSeconds(warningTime);

        SetDamageActive(true);
        yield return new WaitForSeconds(activeTime);

        Destroy(gameObject);
    }

    void SetDamageActive(bool active)
    {
        isActive = active;

        if (damageCollider != null)
            damageCollider.enabled = active;

        bool usesSeparateDamageVisual = damageVisual != null && damageVisual != warningVisual;

        if (warningVisual != null)
        {
            warningVisual.enabled = !active || !usesSeparateDamageVisual;
            warningVisual.color = active ? damageColor : warningColor;
        }
        else
        {
            Debug.LogWarning("DamageWarningZone has no SpriteRenderer assigned, so warning colors cannot be shown.", this);
        }

        if (usesSeparateDamageVisual)
        {
            damageVisual.enabled = active;
            damageVisual.color = damageColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    void TryDamagePlayer(Collider2D other)
    {
        if (!isActive || battle == null) return;

        playerController player = other.GetComponent<playerController>();
        if (player != null)
            battle.DamagePlayer(player);
    }

    void OnDestroy()
    {
        if (battle != null)
            battle.UnregisterWarning(this);
    }
}
