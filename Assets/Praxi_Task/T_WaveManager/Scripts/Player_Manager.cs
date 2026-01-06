using UnityEditor;
using UnityEngine;

public class Player_Manager : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 1f;

    // Target tracking (cached to avoid GetComponent every frame)
    private GameObject currentTarget;
    private EnemyController currentTargetEnemy; // ← CACHED COMPONENT!

    private float lastAttackTime;

    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // No target? Find one
        if (currentTarget == null)
        {
            FindTarget();
            return;
        }

        // Attack

        TryAttackTarget();
    }

    private void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0) return;

        // Find nearest active enemy within range
        float minDistance = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject enemy in enemies)
        {
            // Skip inactive enemies (pooled/dead)
            if (!enemy.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= attackRange && distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }

        if (nearest != null)
        {
            currentTarget = nearest;

            // ✅ CACHE EnemyController component NOW (only once!)
            currentTargetEnemy = currentTarget.GetComponent<EnemyController>();
        }
    }

    private void TryAttackTarget()
    {
        // Target destroyed or inactive?
        if (currentTarget == null || !currentTarget.activeInHierarchy)
        {
            currentTarget = null;
            currentTargetEnemy = null; // Clear cache
            return;
        }

        // Target out of range?
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance > attackRange)
        {
            currentTarget = null;
            currentTargetEnemy = null; // Clear cache
            return;
        }

        // Face target
        FaceTarget();

        // Attack if cooldown finished
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    private void FaceTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void Attack()
    {
        if (currentTarget == null) return;
        lastAttackTime = Time.time;

        if (currentTargetEnemy != null)
        {
            animator.SetBool("Shooting", true);
            currentTargetEnemy.TakeDamage(attackDamage);

            Debug.Log($"[Player] hit {currentTarget.name} for {attackDamage} damage");
        }
        else
        {
            // Component missing, clear target
            currentTarget = null;
            animator.SetBool("Shooting", false);

        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show attack range
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackRange);

        // Show line to target
        if (currentTarget != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}