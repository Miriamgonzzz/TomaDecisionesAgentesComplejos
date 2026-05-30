using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossBlackboard))]
[RequireComponent(typeof(BossActions))]
public class BossBehaviourTree : MonoBehaviour
{
    private BossBlackboard blackboard;
    private BossActions actions;

    private BTTask root;

    private float idleTimer;
    private float patrolTimer;
    private float evadeTimer;
    private float rageTimer;
    private float strongAttackTimer;
    private float rageAttackTimer;

    private bool strongAttackStarted;
    private bool rageAttackStarted;
    private bool evadeStarted;
    private bool rageStarted;
    private bool deathStarted;

    private void Awake()
    {
        blackboard = GetComponent<BossBlackboard>();
        actions = GetComponent<BossActions>();

        BuildTree();
    }

    private void Update()
    {
        if (blackboard.brainMode != BossBrainMode.BehaviourTree)
            return;

        root.Run();
    }

    private void BuildTree()
    {
        root = new BTSelector(new List<BTTask>
        {
            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.isDead),
                new BTAction(Death)
            }),

            new BTRageDecorator(
                blackboard,
                new BTAction(ActivateRage)
            ),

            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => rageStarted),
                new BTAction(RageWait)
            }),

            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => !blackboard.CanSeePlayer()),
                new BTAction(IdleAndPatrol)
            }),

            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.CanSeePlayer()),
                new BTCondition(() => !blackboard.IsPlayerInAttackRange()),
                new BTAction(Chase)
            }),

            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTCondition(() => blackboard.ShouldEvade() || evadeStarted),
                new BTAction(Evade)
            }),

            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTCondition(() => blackboard.rageMode),
                new BTCondition(() => blackboard.ShouldUseSpecialAttack() || rageAttackStarted),
                new BTAction(RageAttack)
            }),

            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTCondition(() => !blackboard.rageMode),
                new BTCondition(() => blackboard.ShouldUseSpecialAttack() || strongAttackStarted),
                new BTAction(StrongAttack)
            }),

            new BTSequence(new List<BTTask>
            {
                new BTCondition(() => blackboard.IsPlayerInAttackRange()),
                new BTAction(NormalAttack)
            })
        });
    }

    private void Death()
    {
        if (deathStarted) return;

        deathStarted = true;
        actions.Die();
        Debug.Log("BT -> Muerte");
    }

    private void ActivateRage()
    {
        if (rageStarted) return;

        rageStarted = true;
        rageTimer = 0f;

        actions.ActivateRageMode();
        Debug.Log("BT -> Activar furia");
    }

    private void RageWait()
    {
        rageTimer += Time.deltaTime;

        if (rageTimer >= 1f)
        {
            rageStarted = false;
        }
    }

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
        Debug.Log("BT -> Chase");
    }

    private void NormalAttack()
    {
        ResetSpecialAttackFlags();

        if (blackboard.CanAttack())
        {
            actions.NormalAttack();
            Debug.Log("BT -> Ataque normal");
        }
    }

    private void StrongAttack()
    {
        if (!strongAttackStarted)
        {
            strongAttackStarted = true;
            strongAttackTimer = 0f;
            actions.StrongAttack();
            Debug.Log("BT -> Ataque fuerte");
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
            Debug.Log("BT -> Ataque fortísimo");
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
            Debug.Log("BT -> Esquivar");
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

    private void ResetSpecialAttackFlags()
    {
        if (!strongAttackStarted)
            strongAttackTimer = 0f;

        if (!rageAttackStarted)
            rageAttackTimer = 0f;
    }
}