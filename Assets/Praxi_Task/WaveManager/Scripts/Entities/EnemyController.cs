using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static WM.Target;

namespace WM
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Type")]
        public EnemyType enemyType;

        [Header("Stats")]
        public float maxHealth = 100f;
        public float currentHealth;
        public float damage = 10f;
        public float attackCooldown = 1.5f;

        private NavMeshAgent agent;
        private Animator animator;

        // Target tracking
        private GameObject currentTarget;
        private Target currentTargetComponent;

        // Runtime
        private float lastAttackTime;
        public bool isDead = false;

        public float myDieAnimationTime = 3f;
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            currentHealth = maxHealth;
        }

        private void OnEnable()
        {
            // Reset NavMeshAgent when enemy is activated from pool
            if (agent != null && gameObject.activeInHierarchy)
            {
                agent.isStopped = false;
                agent.ResetPath();
            }
        }
        void Start()
        {
            FindNearestTarget();
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


        void Update()
        {
            if (isDead) return;

            if (!agent.hasPath) return; // for safety
            // Find target
            if (currentTarget == null)
            {
                //FindNearestTarget();
                return;
            }


            // attack when reach only
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Attack();
            }
        }

        void FindNearestTarget()
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");

            if (targets.Length > 0)
            {
                GameObject nearest = GetNearestFromArray(targets);

                if (nearest != null)
                {
                    currentTarget = nearest;
                    currentTargetComponent = currentTarget.GetComponent<Target>();

                    agent.SetDestination(currentTarget.transform.position);

                    return;
                }
            }
        }

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


        void Attack()
        {
            animator.SetBool("Attack", true);

            if (Time.time < lastAttackTime + attackCooldown) return;

            // Attack
            if (currentTarget != null)
            {
                if (!currentTargetComponent.IsDestroyed)
                {
                    if (currentTargetComponent.targetType == TargetType.Tower)
                        currentTargetComponent.TakeDamage(damage);
                    lastAttackTime = Time.time;

                    // Target destroyed? Clear and find new
                    if (currentTargetComponent.IsDestroyed)
                    {
                        currentTarget = null;
                        currentTargetComponent = null;
                        animator.SetBool("Attack", false);
                        FindNearestTarget();
                    }
                }
                else
                {
                    // Already destroyed, find new target
                    currentTarget = null;
                    currentTargetComponent = null;
                    animator.SetBool("Attack", false);
                    FindNearestTarget();

                }
            }

        }


        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;

            Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            if (isDead) return;

            isDead = true;

            if (agent != null)
            {
                agent.ResetPath();                        // Clear destination to towers
                agent.velocity = Vector3.zero;            // Stop momentum
                agent.SetDestination(transform.position); // Stay at current spot!
                agent.isStopped = true;                   // Stop agent completely
            }

            currentTarget = null;
            currentTargetComponent = null;

            // play death animation
            if (animator != null)
            {
                animator.SetTrigger("Died");
            }


            // Notify SpawnHandler
            if (SpawnHandler.Instance != null)
            {
                SpawnHandler.Instance.OnEnemyDied(gameObject);
            }

            // Deactivate after death animation plays
            StartCoroutine(DeactivateAfterAnimation());
        }


        private IEnumerator DeactivateAfterAnimation()
        {
            yield return new WaitForSeconds(myDieAnimationTime);
            gameObject.SetActive(false);
        }


        public void ResetEnemy()
        {
            // Reset health
            currentHealth = maxHealth;
            isDead = false;

            // Clear target cache
            currentTarget = null;
            currentTargetComponent = null;

            // Reset attack
            lastAttackTime = 0;

            // Reset animator
            if (animator != null && gameObject.activeInHierarchy)
            {
                animator.SetBool("Attack", false);
                animator.Rebind();
                animator.Update(0f);
            }



        }

#if UNITY_EDITOR
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
#endif
    }
}