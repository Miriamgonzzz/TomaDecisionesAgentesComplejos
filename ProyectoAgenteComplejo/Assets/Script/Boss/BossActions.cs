using UnityEngine;

[RequireComponent(typeof(BossBlackboard))]
public class BossActions : MonoBehaviour
{
    //En este script ponemos las acciones reales del agente, así tanto la máquina de estados como el arbol de comportamientos llamarán
    //a estas funciones, consiguiendo un comportamiento equivalente usemos lo que usemos
    private BossBlackboard blackboard;

    private void Awake()
    {
        blackboard = GetComponent<BossBlackboard>();
    }

    public void Idle()
    {
        PlayAnimation(BossAnimationNames.Idle);
    }

    public void Patrol()
    {
        PlayAnimation(BossAnimationNames.Patrol);

        MoveTowards(blackboard.patrolTarget);
            
    }

    public void ChasePlayer()
    {
        if (blackboard.player == null)
        {
            return;
        }

        PlayAnimation(BossAnimationNames.Chase);
        MoveTowards(blackboard.player.position);
    }

    public void NormalAttack()
    {
        if (!blackboard.CanAttack())
        {
            return;
        }

        FacePlayer();
        PlayAnimation(BossAnimationNames.NormalAttack);
        blackboard.RegisterNormalAttack();

        Debug.Log("Robot: ataque normal");
    }

    public void StrongAttack()
    {
        if (!blackboard.CanAttack())
        {
            return;
        }

        FacePlayer();
        PlayAnimation(BossAnimationNames.StrongAttack);
        blackboard.RegisterSpecialAttack();

        Debug.Log("Robot: ataque fuerte");
    }

    public void RageAttack()
    {
        if (!blackboard.CanAttack())
        {
            return;
        }

        FacePlayer();
        PlayAnimation(BossAnimationNames.RageAttack);
        blackboard.RegisterSpecialAttack();

        Debug.Log("Robot: ataque fortísimo");
    }

    public void Evade()
    {

        blackboard.isEvading = true;

        PlayAnimation(BossAnimationNames.Evade);

        Vector3 evadeDirection = -transform.right;
        transform.position += evadeDirection * 2f;

        Debug.Log("Robot: esquiva y no recibe daño");
    }

    public void ActivateRageMode()
    {
        blackboard.ActivateRageMode();
        Debug.Log("Robot: entra en modo furia");
    }

    public void Die()
    {
        PlayAnimation(BossAnimationNames.Death);
        Debug.Log("Robot: muerte");
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 currentPosition = transform.position;

        Vector3 flatTarget = new Vector3(
            target.x,
            currentPosition.y,
            target.z
        );

        Vector3 direction = flatTarget - currentPosition;
        direction.y = 0;

        if (direction.magnitude < 0.2f)
        {
            return;
        }
            

        Vector3 newPosition = currentPosition + direction.normalized * blackboard.moveSpeed * Time.deltaTime;
        newPosition.y = currentPosition.y;

        transform.position = newPosition;

        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            blackboard.rotationSpeed * Time.deltaTime
        );
    }

    private void FacePlayer()
    {
        if (blackboard.player == null)
        {
            return;
        }

        Vector3 direction = blackboard.player.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
            
    }

    private void PlayAnimation(string animationName)
    {
        if (blackboard.animator != null)
        {
            blackboard.animator.Play(animationName);
        }
            
    }
}
