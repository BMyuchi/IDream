using UnityEngine;
using System.Collections;

public class BattleTrigger : MonoBehaviour
{
    public ArenaBattleController battle;
    public float startDelay = 0.35f;
    public bool requirePlayerStayInTrigger = false;

    private bool hasTriggered;
    private bool playerIsInsideTrigger;
    private Coroutine startRoutine;

    void Awake()
    {
        if (battle == null)
            battle = FindAnyObjectByType<ArenaBattleController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (battle == null)
            battle = FindAnyObjectByType<ArenaBattleController>();

        if (battle != null && battle.IsBattleCompleted)
            return;

        if (hasTriggered && battle != null && !battle.IsBattleActive)
            hasTriggered = false;

        if (hasTriggered) return;

        playerController player = other.GetComponentInParent<playerController>();
        if (player == null) return;

        playerIsInsideTrigger = true;

        if (startRoutine == null)
            startRoutine = StartCoroutine(StartBattleAfterDelay(player));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<playerController>() == null) return;

        playerIsInsideTrigger = false;
    }

    IEnumerator StartBattleAfterDelay(playerController player)
    {
        yield return new WaitForSeconds(startDelay);

        startRoutine = null;

        if (hasTriggered || battle == null || battle.IsBattleCompleted)
            yield break;

        if (requirePlayerStayInTrigger && !playerIsInsideTrigger)
            yield break;

        hasTriggered = true;
        battle.StartBattle(player);
    }
}
