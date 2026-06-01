using UnityEngine;

//Importamos los componentes necesarios para que el GameObject pueda funcionar, con el RequireComponent
[RequireComponent(typeof(BossBlackboard))]
[RequireComponent(typeof(BossActions))]
public class BossStateMachine : MonoBehaviour
{
    //Con un enum definimos todos los posibles estados del jefe
    private enum BossState
    {
        Idle,           //Reposo
        Patrol,         //Patrulla por el escenario
        Chase,          //Persecución del jugador
        NormalAttack,   //Ataque básico
        StrongAttack,   //Ataque fuerte para alternar con el ataque básico en modo normal
        RageAttack,     //Ataque fuerte para alternar con el ataque básico en modo furia
        Evade,          //Maniobra de esquive
        RageMode,       //Activación del modo furia
        Death           //Muerte (fin de la actividad del agente)
    }

    [Header("Referencias de Componentes")]
    private BossBlackboard blackboard; //Almacén de datos y condiciones de la IA
    private BossActions actions;       //Controlador de las acciones del jefe

    [Header("Control de Estado")]
    private BossState currentState;    //Estado actual de la IA
    private float stateTimer;          //Temporizador para controlar las transiciones basadas en tiempo
    private bool actionExecuted;       //Variable bandera para asegurar que ciertas acciones se ejecuten solo una vez por estado


    //Iniciamos los componentes
    private void Awake()
    {
        blackboard = GetComponent<BossBlackboard>();
        actions = GetComponent<BossActions>();
    }

    //Inicializamos la máquina de estados en modo Idle
    private void Start()
    {
        ChangeState(BossState.Idle);
    }

    private void Update()
    {
        //Mecanismo de seguridad: Si la IA está configurada en otro modo de decisión (por ejemplo, en el árbol de comportamiento),
        //este cpndicional detiene su ejecución para que las lógicas no entren en conflicto
        if (blackboard.brainMode != BossBrainMode.StateMachine)
        {
            return;
        }

        //Ejecuta y actualiza la lógica del estado actual en cada frame
        UpdateState();
    }

    private void UpdateState()
    {
        //Acumula el tiempo transcurrido en el estado actual
        stateTimer += Time.deltaTime;

        //Si el jefe muere, transiciona inmediatamente a Death, sin importar el estado actual
        if (blackboard.isDead)
        {
            ChangeState(BossState.Death);
            return;
        }

        //Si cumple los requisitos para entrar en furia por perder la mitad de la vida, se activa el modo furia
        if (blackboard.ShouldEnterRage())
        {
            ChangeState(BossState.RageMode);
            return;
        }

        //SWITCH PARA DEFINIR CADA ESTADO POSIBLE
        switch (currentState)
        {
            case BossState.Idle:
                actions.Idle();

                //Si detecta al jugador de manera visual, transiciona al estado perseguir
                if (blackboard.CanSeePlayer())
                {
                    ChangeState(BossState.Chase);
                }
                //Si pasa el tiempo de Idle, pasa a patrullar
                else if (stateTimer >= blackboard.idleDuration)
                {
                    ChangeState(BossState.Patrol);
                }
                break;

            case BossState.Patrol:
                actions.Patrol();

                //Si mientras patrulla ve al jugador, le persigue
                if (blackboard.CanSeePlayer())
                {
                    ChangeState(BossState.Chase);
                }
                //Si pasa el tiempo estipulado patrullando, pasa a Idle
                else if (stateTimer >= blackboard.patrolDuration)
                {
                    ChangeState(BossState.Idle);
                }
                break;

            case BossState.Chase:
                actions.ChasePlayer();

                //Si alcanza la distancia para atacar, pasa al modo ataque básico
                if (blackboard.IsPlayerInAttackRange())
                {
                    ChangeState(BossState.NormalAttack);
                }
                //Si no está cerca del jugador, pasa a Idle
                else if (!blackboard.CanSeePlayer() && blackboard.PlayerIsTooFar())
                {
                    ChangeState(BossState.Idle);
                }
                break;

            case BossState.NormalAttack:
                //Si el jugador se aleja, pasa a perseguirle
                if (!blackboard.IsPlayerInAttackRange())
                {
                    ChangeState(BossState.Chase);
                    return;
                }

                //Si recibe los 3 ataques del jugador, pasa a evadir
                if (blackboard.ShouldEvade())
                {
                    ChangeState(BossState.Evade);
                    return;
                }

                //Dependiendo de si está en modo furia o no, usar un ataque fuerte u otro
                if (blackboard.ShouldUseSpecialAttack())
                {
                    if (blackboard.rageMode)
                        ChangeState(BossState.RageAttack);
                    else
                        ChangeState(BossState.StrongAttack);

                    return;
                }

                //Si el cooldown de ataque se cumple, sigue atacando
                if (blackboard.CanAttack())
                {
                    actions.NormalAttack();
                }
                    

                break;

            case BossState.StrongAttack:
                //Si el jugador se aleja, cancela el ataque especial
                if (!blackboard.IsPlayerInAttackRange())
                {
                    ChangeState(BossState.Chase);
                    return;
                }

                //Aseguramos que el ataque especial solo se ejecute una vez
                if (!actionExecuted)
                {
                    actions.StrongAttack();
                    actionExecuted = true;
                }

                //Una vez concluído el tiempo del ataque especial, vuelve al ataque básico
                if (stateTimer >= blackboard.strongAttackDuration)
                {
                    ChangeState(BossState.NormalAttack);
                }
                break;

            case BossState.RageAttack:
                if (!blackboard.IsPlayerInAttackRange())
                {
                    ChangeState(BossState.Chase);
                    return;
                }

                //Misma lógica del StrongAttack
                if (!actionExecuted)
                {
                    actions.RageAttack();
                    actionExecuted = true;
                }

                
                if (stateTimer >= blackboard.rageAttackDuration)
                {
                    ChangeState(BossState.NormalAttack);
                }
                break;

            case BossState.Evade:
                
                if (!actionExecuted)
                {
                    actions.Evade();
                    actionExecuted = true;
                }

                //Al terminar la ventana de tiempo del esquive (0.8s), limpia los booleanos en la pizarra y contraataca
                if (stateTimer >= 0.8f)
                {
                    blackboard.isEvading = false;
                    blackboard.RegisterEvade(); //Registra el tiempo que dura la evasión
                    ChangeState(BossState.NormalAttack);
                }
                break;

            case BossState.RageMode:
                actions.ActivateRageMode();

                if (stateTimer >= 1f)
                {
                    ChangeState(BossState.Chase);
                }
                break;

            case BossState.Death:
                //Si muere, detiene la IA
                actions.Die();
                enabled = false; //Congelamos la máquina de estados
                break;
        }
    }

    //Método para gestionar la transición segura entre los estados de la IA, reseteando temporizadores
    private void ChangeState(BossState newState)
    {
        //Evita volver a entrar en el mismo estado innecesariamente
        if (currentState == newState)
        {
            return;
        }

        //Ejecuta la lógica de salida del estado previo
        OnExitState(currentState);

        //Actualiza el estado y resetea las variables de control de tiempo y ejecución
        currentState = newState;
        stateTimer = 0f;
        actionExecuted = false;

        //Ejecuta la lógica de entrada del nuevo estado
        OnEnterState(currentState);
    }

    //Callback que se ejecuta inmediatamente al entrar a un estado específico
    private void OnEnterState(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                actions.Idle();
                break;

            case BossState.Patrol:
                blackboard.ChooseNewPatrolTarget(); //Define el siguiente punto de ruta antes de moverse a patrullar
                actions.Patrol();
                break;

            case BossState.RageMode:
                actions.ActivateRageMode();
                break;

            case BossState.Death:
                actions.Die();
                break;
        }
    }

    //Callback que se ejecuta justo antes de abandonar el estado. Lo usamos para el debugging
    private void OnExitState(BossState state)
    {
        switch (state)
        {
            case BossState.Patrol:
                Debug.Log("FSM -> Sale de patrulla");
                break;

            case BossState.NormalAttack:
                Debug.Log("FSM -> Sale de ataque normal");
                break;
        }
    }
}