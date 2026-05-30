using UnityEngine;

[RequireComponent(typeof(BossBlackboard))]
[RequireComponent(typeof(BossActions))]
public class BossStateMachine : MonoBehaviour
{
    //Maquina de estados
    private enum BossState
    {
        Idle,
        Patrol,
        Chase,
        NormalAttack,
        StrongAttack,
        RageAttack,
        Evade,
        RageMode,
        Death
    }

    private BossBlackboard blackboard;
    private BossActions actions;

    private BossState currentState;
    private float stateTimer;
    private bool actionExecuted;

    private void Awake()
    {
        blackboard = GetComponent<BossBlackboard>();
        actions = GetComponent<BossActions>();
    }

    private void Start()
    {
        ChangeState(BossState.Idle);
    }

    private void Update()
    {
        if (blackboard.brainMode != BossBrainMode.StateMachine)
        {
            return;
        }
            

        UpdateState();
    }

    private void UpdateState()
    {
        stateTimer += Time.deltaTime;

        if (blackboard.isDead)
        {
            ChangeState(BossState.Death);
            return;
        }

        if (blackboard.ShouldEnterRage())
        {
            ChangeState(BossState.RageMode);
            return;
        }

        switch (currentState)
        {
            case BossState.Idle:
                actions.Idle();

                if (blackboard.CanSeePlayer())
                {
                    ChangeState(BossState.Chase);
                }
                else if (stateTimer >= blackboard.idleDuration)
                {
                    ChangeState(BossState.Patrol);
                }
                break;

            case BossState.Patrol:
                actions.Patrol();

                if (blackboard.CanSeePlayer())
                {
                    ChangeState(BossState.Chase);
                }
                else if (stateTimer >= blackboard.patrolDuration)
                {
                    ChangeState(BossState.Idle);
                }
                break;

            case BossState.Chase:
                actions.ChasePlayer();

                if (blackboard.IsPlayerInAttackRange())
                {
                    ChangeState(BossState.NormalAttack);
                }
                else if (!blackboard.CanSeePlayer() && blackboard.PlayerIsTooFar())
                {
                    ChangeState(BossState.Idle);
                }
                break;

            case BossState.NormalAttack:
                if (!blackboard.IsPlayerInAttackRange())
                {
                    ChangeState(BossState.Chase);
                    return;
                }

                if (blackboard.ShouldEvade())
                {
                    ChangeState(BossState.Evade);
                    return;
                }

                if (blackboard.ShouldUseSpecialAttack())
                {
                    if (blackboard.rageMode)
                        ChangeState(BossState.RageAttack);
                    else
                        ChangeState(BossState.StrongAttack);

                    return;
                }

                if (blackboard.CanAttack())
                    actions.NormalAttack();

                break;

            case BossState.StrongAttack:
                if (!blackboard.IsPlayerInAttackRange())
                {
                    ChangeState(BossState.Chase);
                    return;
                }

                if (!actionExecuted)
                {
                    actions.StrongAttack();
                    actionExecuted = true;
                }

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

                if (stateTimer >= 0.8f)
                {
                    blackboard.isEvading = false;
                    blackboard.RegisterEvade();
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
                actions.Die();
                enabled = false;
                break;
        }
    }

    private void ChangeState(BossState newState)
    {
        if (currentState == newState)
            return;

        OnExitState(currentState);

        currentState = newState;
        stateTimer = 0f;
        actionExecuted = false;

        OnEnterState(currentState);

        Debug.Log("FSM -> Estado actual: " + currentState);
    }

    private void OnEnterState(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                actions.Idle();
                break;

            case BossState.Patrol:
                blackboard.ChooseNewPatrolTarget();
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
