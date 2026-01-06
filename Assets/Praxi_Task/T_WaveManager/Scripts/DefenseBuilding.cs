using UnityEditor;
using UnityEngine;

/// <summary>
/// Defense building that attacks enemies in range.
/// Optimized with cached components.
/// </summary>
public class DefenseBuilding : Building
{
    // ==================== ATTACK STATS ====================

    [Header("Defense Stats")]
    public float attackDamage = 15f;
    public float attackRange = 10f;
    public float attackCooldown = 1f;

    // Components
    private Animator animator;

    // Target tracking (cached to avoid GetComponent every frame)
    private GameObject currentTarget;
    private EnemyController currentTargetEnemy; // ← CACHED COMPONENT!

    // Runtime
    private float lastAttackTime;

    // ==================== INITIALIZATION ====================

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    // ==================== UPDATE LOOP ====================

    private void Update()
    {
        if (IsDestroyed) return;

        // No target? Find one
        if (currentTarget == null)
        {
            FindTarget();
            return;
        }

        // Has target? Try to attack
        TryAttackTarget();
    }

    // ==================== TARGETING SYSTEM ====================

    /// <summary>
    /// Find enemy by tag within attack range.
    /// Caches EnemyController component.
    /// </summary>
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

            // Within range?
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

            Debug.Log($"[{gameObject.name}] locked on to {currentTarget.name}");
        }
    }

    /// <summary>
    /// Check if target still valid and attack if ready.
    /// </summary>
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

    /// <summary>
    /// Rotate to face target.
    /// </summary>
    private void FaceTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        direction.y = 0; // Keep horizontal

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // ==================== ATTACK SYSTEM ====================

    /// <summary>
    /// Attack current target using CACHED component.
    /// </summary>
    private void Attack()
    {
        if (currentTarget == null) return;

        lastAttackTime = Time.time;

        // ✅ Use CACHED component (NO GetComponent call!)
        if (currentTargetEnemy != null)
        {
            currentTargetEnemy.TakeDamage(attackDamage);

            // Play attack animation
            if (animator != null)
            {
                animator.SetBool("Shooting",true);
            }

            Debug.Log($"[{gameObject.name}] hit {currentTarget.name} for {attackDamage} damage");
        }
        else
        {
            // Component missing, clear target
            currentTarget = null;
            animator.SetBool("Shooting", false);

        }
    }

    // ==================== EDITOR VISUALIZATION ====================

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