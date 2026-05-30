using UnityEngine;

public class BossBlackboard : MonoBehaviour
{
    //Es el “cerebro de datos”, centraliza todos los datos del agente necesarios para realizar las distintas acciones

    [Header("Referencias")]
    public Transform player;
    public Animator animator;
    public Renderer robotRenderer;
    public Material normalMaterial;
    public Material rageMaterial;

    [Header("Modo de IA")]
    public BossBrainMode brainMode = BossBrainMode.StateMachine;

    [Header("Vida")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Movimiento")]
    public float moveSpeed = 2.5f;
    public float rageMoveSpeed = 3.8f;
    public float rotationSpeed = 8f;

    [Header("Rangos")]
    public float detectionRange = 12f;
    public float attackRange = 5f;
    public float loseInterestRange = 18f;

    [Header("Patrulla")]
    public float idleDuration = 3f;
    public float patrolDuration = 4f;
    public float patrolRadius = 4f;

    [Header("Combate")]
    public int normalAttacksBeforeSpecial = 5;
    public int playerHitsBeforeEvade = 3;
    public float attackCooldown = 1.4f;

    [Header("Duración de ataques")]
    public float normalAttackDuration = 1.0f;
    public float strongAttackDuration = 1.8f;
    public float rageAttackDuration = 2.2f;

    [HideInInspector] public bool rageMode;
    [HideInInspector] public bool isDead;
    [HideInInspector] public bool isEvading;
    [HideInInspector] public int normalAttackCounter;
    [HideInInspector] public int receivedHitsCounter;
    [HideInInspector] public float lastAttackTime;
    [HideInInspector] public Vector3 spawnPosition;
    [HideInInspector] public Vector3 patrolTarget;

    private void Awake()
    {
        spawnPosition = transform.position;
        currentHealth = maxHealth;
        ChooseNewPatrolTarget();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
            
    }

    public float DistanceToPlayer()
    {
        if (player == null)
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(transform.position, player.position);
    }

    public bool CanSeePlayer()
    {
        return DistanceToPlayer() <= detectionRange;
    }

    public bool IsPlayerInAttackRange()
    {
        return DistanceToPlayer() <= attackRange;
    }

    public bool PlayerIsTooFar()
    {
        return DistanceToPlayer() >= loseInterestRange;
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public bool ShouldEnterRage()
    {
        return !rageMode && currentHealth <= maxHealth * 0.5f;
    }

    public bool ShouldEvade()
    {
        return receivedHitsCounter >= playerHitsBeforeEvade;
    }

    public bool ShouldUseSpecialAttack()
    {
        return normalAttackCounter >= normalAttacksBeforeSpecial;
    }

    public void RegisterNormalAttack()
    {
        normalAttackCounter++;
        lastAttackTime = Time.time;
    }

    public void RegisterSpecialAttack()
    {
        normalAttackCounter = 0;
        lastAttackTime = Time.time;
    }

    public void RegisterEvade()
    {
        receivedHitsCounter = 0;
    }

    public void ReceiveDamage(float damage)
    {
        if (isDead || isEvading)
        {
            return;
        }

        currentHealth -= damage;
        receivedHitsCounter++;

        Debug.Log("Vida robot: " + currentHealth);
        Debug.Log("Golpes recibidos para esquivar: " + receivedHitsCounter);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
    }

    public void ActivateRageMode()
    {
        if (rageMode)
        {
            return;
        }

        rageMode = true;
        moveSpeed = rageMoveSpeed;
        attackCooldown = 1.0f;

        if (robotRenderer != null && rageMaterial != null)
        {
            robotRenderer.material = rageMaterial;
        }
            
    }

    public void ChooseNewPatrolTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;

        patrolTarget = new Vector3(
            spawnPosition.x + randomCircle.x,
            transform.position.y,
            spawnPosition.z + randomCircle.y
        );

        Debug.Log("Nuevo punto de patrulla: " + patrolTarget);
    }
}
