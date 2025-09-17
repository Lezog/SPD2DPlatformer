using UnityEngine;

public class BossMeleeAreaCheck : MonoBehaviour
{
    private BossEnemy boss;

    private void Awake()
    {
        boss = GetComponentInParent<BossEnemy>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss.OnAttackAreaEnter(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss.OnAttackAreaExit(other);
        }
    }
}
