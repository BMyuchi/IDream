using UnityEngine;

public class BattleReflectItem : MonoBehaviour
{
    public int damage = 1;
    public float flySpeed = 8f;

    private EnemyHealth targetEnemy;
    private ArenaBattleController battle;
    private bool isFlyingToEnemy;

    public void Initialize(EnemyHealth enemy)
    {
        Initialize(enemy, null);
    }

    public void Initialize(EnemyHealth enemy, ArenaBattleController owner)
    {
        targetEnemy = enemy;
        battle = owner;
    }

    void Update()
    {
        if (!isFlyingToEnemy || targetEnemy == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetEnemy.transform.position,
            flySpeed * Time.deltaTime
        );

        if ((transform.position - targetEnemy.transform.position).sqrMagnitude < 0.0225f)
            HitEnemy();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isFlyingToEnemy) return;

        playerController player = other.GetComponent<playerController>();
        if (player != null)
            isFlyingToEnemy = true;
    }

    void HitEnemy()
    {
        if (targetEnemy != null)
            targetEnemy.TakeDamage(damage);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (battle != null)
            battle.UnregisterReflectItem(this);
    }
}
