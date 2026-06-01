using UnityEngine;

//Actúa como el "cerebro de datos" o Pizarra (Blackboard) del Agente. 
//Centraliza todas las variables, parámetros de diseño y estados dinámicos del Jefe
//para que puedan ser consultados y modificados por la Máquina de Estados o el Árbol de Comportamiento
public class BossBlackboard : MonoBehaviour
{
    [Header("Referencias Externas")]
    public Transform player;
    public Animator animator;
    public Renderer robotRenderer;
    public Material normalMaterial;
    public Material rageMaterial;

    [Header("Configuración del Modo de IA")]
    public BossBrainMode brainMode = BossBrainMode.StateMachine;

    [Header("Atributos de Vida")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Parámetros de Movimiento")]
    public float moveSpeed = 2.5f;
    public float rageMoveSpeed = 3.8f;
    public float rotationSpeed = 8f;

    [Header("Rangos y Distancias (Círculos de Control)")]
    public float detectionRange = 12f;
    public float attackRange = 5f;
    public float loseInterestRange = 18f;

    [Header("Configuración de Patrulla")]
    public float idleDuration = 3f;
    public float patrolDuration = 4f;
    public float patrolRadius = 4f;

    [Header("Reglas de Combate")]
    public int normalAttacksBeforeSpecial = 5;
    public int playerHitsBeforeEvade = 3;
    public float attackCooldown = 1.4f;

    [Header("Duración de las Acciones de Ataque")]
    public float normalAttackDuration = 1.0f;
    public float strongAttackDuration = 1.8f;
    public float rageAttackDuration = 2.2f;

    //Variable de estado internas, ocultas en el inspector
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

    //Calcula la distancia entre el Jefe y el Jugador
    public float DistanceToPlayer()
    {
        if (player == null)
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(transform.position, player.position);
    }

  
    //Evalúa si el jugador está dentro del rango de detección visual del Jefe
    public bool CanSeePlayer()
    {
        return DistanceToPlayer() <= detectionRange;
    }

    //Evalúa si el jugador está lo suficientemente cerca para iniciar las acciones ofensivas
    public bool IsPlayerInAttackRange()
    {
        return DistanceToPlayer() <= attackRange;
    }

    //Condición de abandono: Determina si el jugador se ha alejado más allá del rango de interés
    public bool PlayerIsTooFar()
    {
        return DistanceToPlayer() >= loseInterestRange;
    }

    //Comprueba mediante el reloj global de Unity si ha transcurrido el cooldown desde el último ataque
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    //Comprobación de la salud del jefe para ver si entra en modo furia o no (menos del 50% de vida, entra en furia)
    public bool ShouldEnterRage()
    {
        return !rageMode && currentHealth <= (maxHealth * 0.5f);
    }

    //Comprueba si puede esquivar (si ha recibido ya los ataques pertinentes del Jugador, esquiva)
    public bool ShouldEvade()
    {
        return receivedHitsCounter >= playerHitsBeforeEvade;
    }

    //Comprueba si se ha ejecutado la cantidad de ataques normales requeridos para desencadenar el ataque especial
    public bool ShouldUseSpecialAttack()
    {
        return normalAttackCounter >= normalAttacksBeforeSpecial;
    }

    //Registra la ejecución de un ataque básico incrementando el contador y actualizando el temporizador de control
    public void RegisterNormalAttack()
    {
        normalAttackCounter++;
        lastAttackTime = Time.time;
    }

    //Resetea el contador de ataques normales tras haber ejecutado con éxito un ataque fuerte
    public void RegisterSpecialAttack()
    {
        normalAttackCounter = 0;
        lastAttackTime = Time.time;
    }

    //Resetea el contador de golpes recibidos una vez usada la evasión
    public void RegisterEvade()
    {
        receivedHitsCounter = 0;
    }

    //Aplica daño al Jefe gestionando las restricciones de invulnerabilidad por evasión o muerte
    public void ReceiveDamage(float damage)
    {
        //Si ya está muerto o se encuentra esquivando, se ignora el daño
        if (isDead || isEvading)
        {
            return;
        }

        currentHealth -= damage;
        receivedHitsCounter++;

        //Si la vida baja por debajo o llega a 0, se pone a 0 y pasa a estar muerto
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
    }

    //Altera de manera permanente las estadísticas de combate del Jefe e intercambia su material para reflejar visualmente el estado de furia
    public void ActivateRageMode()
    {
        //evitamos re-renderizar de forma innecesaria
        if (rageMode)
        {
            return;
        }

        rageMode = true;
        moveSpeed = rageMoveSpeed; //Aumenta la velocidad de movimiento
        attackCooldown = 1.0f;     //Reduce el cooldown de ataque para volverse más agresivo

        //Cambiamos el material del robot (amarillo por rojo)
        if (robotRenderer != null && rageMaterial != null)
        {
            robotRenderer.material = rageMaterial;
        }
    }

    //Genera una coordenada tridimensional aleatoria en el plano horizontal (X, Z) contenida dentro 
    //de un radio establecido a partir del punto de origen (spawnPosition) para patrullar hacia una zona nueva
    //Una lógica extraída de los algoritmos de movimiento cinemático, de deambular, efectiva para el Jefe
    public void ChooseNewPatrolTarget()
    {
        //Se genera un punto aleatorio en un círculo imaginario 2D
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;

        //Se traslada ese punto al espacio 3D mapeando la variable Y del círculo en el eje global Z de Unity
        patrolTarget = new Vector3(
            spawnPosition.x + randomCircle.x,
            transform.position.y,
            spawnPosition.z + randomCircle.y
        );

    }
}