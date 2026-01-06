using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy finds and attacks nearest building.
/// Optimized with cached components and object pooling.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damage = 10f;
    public float attackCooldown = 1.5f;

    // Components (cached)
    private NavMeshAgent agent;
    private Animator animator;

    // Target tracking (cached to avoid GetComponent every frame)
    private GameObject currentTarget;
    private Building currentTargetBuilding; // ← CACHED COMPONENT (saves memory!)
    private string currentTargetTag;

    // Runtime
    private float lastAttackTime;
    private bool isDead = false;

    // ==================== INITIALIZATION ====================

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Auto-detect type from stopping distance
        if (agent.stoppingDistance > 5f)
        {
            enemyType = EnemyType.Ranged;
        }
        else
        {
            enemyType = EnemyType.Melee;
        }
    }

    // ==================== UPDATE ====================

    void Update()
    {
        if (isDead) return;

        // No target? Find one
        if (currentTarget == null)
        {
            FindNearestBuilding();
            return;
        }

        // Safety check
        if (!agent.hasPath) return;

        // Reached target? Attack!
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Attack();
        }else
        {
            animator.SetBool("Attack", false);

        }
    }

    // ==================== TARGETING SYSTEM ====================

    /// <summary>
    /// Find nearest building or player.
    /// Caches Building component to avoid GetComponent every frame.
    /// </summary>
    void FindNearestBuilding()
    {
        // Priority 1: Find buildings
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        if (buildings.Length > 0)
        {
            GameObject nearest = GetNearestFromArray(buildings);

            if (nearest != null)
            {
                currentTarget = nearest;
                currentTargetTag = "Building";

                // ✅ CACHE Building component NOW (only once per target!)
                currentTargetBuilding = currentTarget.GetComponent<Building>();

                agent.SetDestination(currentTarget.transform.position);

                Debug.Log($"{gameObject.name} targeting building: {currentTarget.name}");
                return;
            }
        }

        // Priority 2: Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            currentTarget = player;
            currentTargetTag = "Player";
            currentTargetBuilding = null; // Player has no Building component

            agent.SetDestination(currentTarget.transform.position);

            Debug.Log($"{gameObject.name} targeting Player!");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} - No targets found!");
        }
    }

    /// <summary>
    /// Get nearest GameObject from array.
    /// </summary>
    GameObject GetNearestFromArray(GameObject[] objects)
    {
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;

            float distance = Vector3.Distance(transform.position, obj.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = obj;
            }
        }

        return nearest;
    }

    // ==================== COMBAT SYSTEM ====================

    /// <summary>
    /// Attack current target.
    /// Uses cached Building component (NO GetComponent call!).
    /// </summary>
    void Attack()
    {
        animator.SetBool("Attack", true);

        if (Time.time < lastAttackTime + attackCooldown) return;

        // Attack building using CACHED component
        if (currentTargetBuilding != null)
        {
            if (!currentTargetBuilding.IsDestroyed)
            {
                currentTargetBuilding.TakeDamage(damage);
                lastAttackTime = Time.time;

                Debug.Log($"{gameObject.name} attacked {currentTarget.name} for {damage} damage");

                // Target destroyed? Clear and find new
                if (currentTargetBuilding.IsDestroyed)
                {
                    currentTarget = null;
                    currentTargetBuilding = null; // Clear cache
                }
            }
            else
            {
                // Already destroyed, find new target
                currentTarget = null;
                currentTargetBuilding = null; // Clear cache
            }
        }
        // Attack player
        else if (currentTargetTag == "Player" && currentTarget != null)
        {
            // TODO: Add player damage here
            // PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
            // playerHealth.TakeDamage(damage);

            lastAttackTime = Time.time;
            Debug.Log($"{gameObject.name} attacked Player for {damage} damage");
        }
    }

    /// <summary>
    /// Take damage from defense buildings.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    // ==================== DEATH & POOLING ====================

    /// <summary>
    /// Enemy dies - uses object pooling instead of Destroy.
    /// </summary>
    IEnumerator Die()
    {
        isDead = true;
        agent.isStopped = true;
        animator.SetTrigger("Died");

        yield return new WaitForSeconds(3);
        // Notify SpawnHandler
        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.OnEnemyDied(gameObject);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Reset enemy to fresh state when reused from pool.
    /// </summary>
    public void ResetEnemy()
    {
        // Reset health
        currentHealth = maxHealth;
        isDead = false;

        // Clear target cache
        currentTarget = null;
        currentTargetBuilding = null; // ← Clear cached component!
        currentTargetTag = null;

        // Reset attack
        lastAttackTime = 0;

        // Reset animator
        if (animator != null)
        {
            animator.SetBool("Attack", false);
            animator.Rebind();
            animator.Update(0f);
        }

        // Reset NavMeshAgent
        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        Debug.Log($"{gameObject.name} reset and ready!");
    }

    // ==================== EDITOR VISUALIZATION ====================

    private void OnDrawGizmosSelected()
    {
        // Show attack range
        if (agent != null)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.up, agent.stoppingDistance);
        }

        // Show line to target
        if (currentTarget != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}