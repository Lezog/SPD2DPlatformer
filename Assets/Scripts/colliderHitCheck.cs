using UnityEngine;

public class colliderHitCheck : MonoBehaviour
{
    private BossEnemy boss;

    private void Awake()
    {
        boss = GetComponentInParent<BossEnemy>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss.swingAttack();
        }
    }

}
