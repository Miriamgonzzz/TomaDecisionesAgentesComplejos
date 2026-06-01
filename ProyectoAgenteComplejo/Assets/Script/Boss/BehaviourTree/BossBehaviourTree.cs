using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossBlackboard))]
[RequireComponent(typeof(BossActions))]
public class BossBehaviourTree : MonoBehaviour
{
    private BossBlackboard blackboard;
    private BossActions actions;       

    private BTTask root;               //Nodo raíz que comanda el árbol de comportamientos

    [Header("Temporizadores de Bloqueo Interno")]
    private float idleTimer;
    private float patrolTimer;
    private float evadeTimer;
    private float rageTimer;
    private float strongAttackTimer;
    private float rageAttackTimer;

    [Header("Flags de Control de Acciones Persistentes")]
    private bool strongAttackStarted;
    private bool rageAttackStarted;
    private bool evadeStarted;
    private bool rageStarted;
    private bool deathStarted;

    private void Awake()
    {
        blackboard = GetComponent<BossBlackboard>();
        actions = GetComponent<BossActions>();

        //Construye la estructura jerárquica del árbol al comenzar
        BuildTree();
    }

    private void Update()
    {
        //Mecanismo de segurida. Solo ejecuta el árbol si la IA está configurada en este modo mediante el Enum
        if (blackboard.brainMode != BossBrainMode.BehaviourTree)
            return;

        //Evalúa toda la estructura desde la raíz en cada frame
        root.Run();
    }

    //Construye y define la topología jerárquica del Árbol de Comportamiento
    //Utiliza un Selector principal combinado con Secuencias (condición + acción)
    private void BuildTree()
    {
        //El nodo raíz es un Selector. Evalua a sus hijos de arriba a abajo y ejecuta el primero que tenga éxito
        root = new BTSelector(new List<BTTask>
        {
            //PRIORIDAD 1: Condición de Muerte global
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.isDead),
                new BTAction(Death)
            }),

            //PRIORIDAD 2: Decorador de Furia (Si se puede pasar al modo furia)
            new BTRageDecorator(
                blackboard,
                new BTAction(ActivateRage)
            ),

            //PRIORIDAD 3: Bloqueo por activación de furia
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => rageStarted),
                new BTAction(RageWait)
            }),

            //PRIORIDAD 4: Estado de Idle/Patrulla si no ve al jugador
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => !blackboard.CanSeePlayer()),
                new BTAction(IdleAndPatrol)
            }),

            //PRIORIDAD 5: Persecución, si ve al Jugador pero está fuera de alcance para atacarle
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.CanSeePlayer()),
                new BTCondition(() => !blackboard.IsPlayerInAttackRange()),
                new BTAction(Chase)
            }),

            //PRIORIDAD 6: Maniobra de Evasión
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTCondition(() => blackboard.ShouldEvade() || evadeStarted), //Se mantiene activo si ya empezó, para evitar cortar la animación
                new BTAction(Evade)
            }),

            //PRIORIDAD 7: Ataque Fuerte en modo furia
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTCondition(() => blackboard.rageMode),
                new BTCondition(() => blackboard.ShouldUseSpecialAttack() || rageAttackStarted), //se mantiene y no corta la animación hasta finalizarla
                new BTAction(RageAttack)
            }),

            // PRIORIDAD 8: Ataque Fuerte en modo normal
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTCondition(() => !blackboard.rageMode),
                new BTCondition(() => blackboard.ShouldUseSpecialAttack() || strongAttackStarted), //se mantiene y no corta la animación hasta acabarla
                new BTAction(StrongAttack)
            }),

            //PRIORIDAD 9: Ataque básico por defecto
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTAction(NormalAttack)
            })
        });
    }


    //método para matar al Jefe
    private void Death()
    {
        if (deathStarted)
        {
            return;
        }

        deathStarted = true;
        actions.Die();
    }

    //método para activar la furia
    private void ActivateRage()
    {
        if (rageStarted) 
        {
            return;
        }
        

        rageStarted = true;
        rageTimer = 0f;

        actions.ActivateRageMode();
    }

    //Simula un nodo temporal de espera. Sostiene la ejecución del Jefe durante 1s mientras cambia a la fase furia
    private void RageWait()
    {
        rageTimer += Time.deltaTime;

        if (rageTimer >= 1f)
        {
            rageStarted = false; //Libera el bloqueo para permitir otros comportamientos en el siguiente frame
        }
    }

    //Método para controlar el ciclo alterno entre Idle y patrullar sin salir del nodo de acción
    private void IdleAndPatrol()
    {
        
        if (patrolTimer > 0f)
        {
            patrolTimer += Time.deltaTime;
            actions.Patrol();

            if (patrolTimer >= blackboard.patrolDuration)
            {
                patrolTimer = 0f;
                idleTimer = 0f;
            }
            return;
        }

        idleTimer += Time.deltaTime;
        actions.Idle();

        if (idleTimer >= blackboard.idleDuration)
        {
            blackboard.ChooseNewPatrolTarget(); 
            patrolTimer = 0.01f; 
        }
    }

    private void Chase()
    {
        ResetAttackTimers();
        actions.ChasePlayer();
    }

    private void NormalAttack()
    {
        ResetSpecialAttackFlags();

        if (blackboard.CanAttack())
        {
            actions.NormalAttack();
        }
    }

    
    private void StrongAttack()
    {
        if (!strongAttackStarted)
        {
            strongAttackStarted = true;
            strongAttackTimer = 0f;
            actions.StrongAttack();
        }

        strongAttackTimer += Time.deltaTime;

        if (strongAttackTimer >= blackboard.strongAttackDuration)
        {
            strongAttackStarted = false; 
            strongAttackTimer = 0f;
        }
    }

    private void RageAttack()
    {
        if (!rageAttackStarted)
        {
            rageAttackStarted = true;
            rageAttackTimer = 0f;
            actions.RageAttack();
        }

        rageAttackTimer += Time.deltaTime;

        if (rageAttackTimer >= blackboard.rageAttackDuration)
        {
            rageAttackStarted = false;
            rageAttackTimer = 0f;
        }
    }

   
    private void Evade()
    {
        if (!evadeStarted)
        {
            evadeStarted = true;
            evadeTimer = 0f;
            actions.Evade();
        }

        evadeTimer += Time.deltaTime;

        if (evadeTimer >= 0.8f)
        {
            blackboard.isEvading = false;
            blackboard.RegisterEvade();
            evadeStarted = false;
            evadeTimer = 0f;
        }
    }

    //Resetea todos los temporizadores y variables bandera pasados 
    //Utilizado al cambiar bruscamente de comportamiento (por ejemplo cuando pasa de ataque a perseguir)
    private void ResetAttackTimers()
    {
        strongAttackStarted = false;
        rageAttackStarted = false;
        evadeStarted = false;

        strongAttackTimer = 0f;
        rageAttackTimer = 0f;
        evadeTimer = 0f;

        blackboard.isEvading = false;
    }

    //Resetea los cronómetros de los ataques especiales si estos no han comenzado
    private void ResetSpecialAttackFlags()
    {
        if (!strongAttackStarted)
        {
            strongAttackTimer = 0f;
        }
            

        if (!rageAttackStarted)
        {
            rageAttackTimer = 0f;
        }
            
    }
}