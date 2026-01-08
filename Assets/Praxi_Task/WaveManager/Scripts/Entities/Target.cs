using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using static WM.Target;

namespace WM
{
    public class Target : MonoBehaviour
    {


        public enum TargetType
        {
            Tower, Player
        }
        [Header("Target Identity")]
        public TargetType targetType;

        [Header("Health")]
        [ShowIf(nameof(IsTower))]
        public float maxHealth = 200f;
        [ShowIf(nameof(IsTower))]
        public float currentHealth;
        [HideInInspector] public bool IsDestroyed = false;

        [Header("Attack Parameters")]
        public float attackDamage = 15f;
        public float attackRange = 10f;
        public float attackCooldown = 1f;



        [Header("Attack Attributes")]
        private Animator animator;
        private float lastAttackTime;
        private GameObject currentTarget;
        private EnemyController currentTargetEnemy;


        protected virtual void Awake()
        {
            // Initialize health
            currentHealth = maxHealth;


        }

        private void Start()
        {
            if (targetType == TargetType.Player)
            {
                animator = GetComponentInChildren<Animator>();
            }
            else if (targetType == TargetType.Tower)
                animator = GetComponent<Animator>();
        }
        private void Update()
        {
            if (IsDestroyed) return;

            // Find one target
            if (currentTarget == null)
            {
                FindTarget();
                return;
            }

            // Try to attack
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
                currentTargetEnemy = currentTarget.GetComponent<EnemyController>();



            }
        }


        private void TryAttackTarget()
        {
            if (IsDestroyed) return;
            // Target destroyed or inactive?
            if (currentTarget == null || !currentTarget.activeInHierarchy || currentTargetEnemy.isDead)
            {
                currentTarget = null;
                currentTargetEnemy = null; // Clear cache
                if (gameObject.activeInHierarchy)
                    animator.SetBool("Shooting", false);

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
            direction.y = 0; // Keep horizontal

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
                currentTargetEnemy.TakeDamage(attackDamage);

                // Play attack animation
                if (animator != null)
                {
                    animator.SetBool("Shooting", true);
                }

            }
            else
            {
                animator.SetBool("Shooting", false);

            }
        }

        public void ResetTarget()
        {
            currentTarget = null;
            currentTargetEnemy = null;

            // Stop attack animation
            if (animator != null)
            {
                animator.SetBool("Shooting", false);
            }

        }



        public virtual void TakeDamage(float damage)
        {
            if (IsDestroyed) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);



            if (currentHealth <= 0)
            {
                Destroyed();
            }
        }




        protected virtual void Destroyed()
        {
            if (IsDestroyed) return;

            IsDestroyed = true;




            //To Do: Play destruction effects, particles, sound

            // turn off GameObject
            gameObject.SetActive(false);
        }


#if UNITY_EDITOR
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
#endif
        private bool IsTower()
        {
            return targetType == TargetType.Tower;
        }
    }

}