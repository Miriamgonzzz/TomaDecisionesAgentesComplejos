using UnityEngine;

[RequireComponent(typeof(BossBlackboard))]

//Controlador de ejecución de acciones físicas y animaciones del Jefe
//Este script encapsula cómo hace las cosas el agente, permitiendo que tanto la Máquina de estados
//como el Árbol de Comportamiento invoquen estas funciones para lograr resultados idénticos, evitando la
//duplicación de código innecesario
public class BossActions : MonoBehaviour
{
    private BossBlackboard blackboard;

    private void Awake()
    {
        blackboard = GetComponent<BossBlackboard>();
    }

    //Métodos para activar las animaciones de los distintos estados del Jefe e invocar los métodos de actuación
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
    }

    public void Evade()
    {
        blackboard.isEvading = true;

        PlayAnimation(BossAnimationNames.Evade);
        Vector3 evadeDirection = -transform.right;
        transform.position += evadeDirection * 2f;
    }

    public void ActivateRageMode()
    {
        blackboard.ActivateRageMode();
    }

    public void Die()
    {
        PlayAnimation(BossAnimationNames.Death);
    }

    //Método para mover al Jefe hacia un punto determinado
    private void MoveTowards(Vector3 target)
    {
        Vector3 currentPosition = transform.position;

        //Se aplana la coordenada Y del objetivo para evitar que el Jefe intente volar o inclinarse hacia el suelo
        Vector3 flatTarget = new Vector3(
            target.x,
            currentPosition.y,
            target.z
        );

        //Obtiene el vector dirección restando Destino - Origen
        Vector3 direction = flatTarget - currentPosition;
        direction.y = 0; //Doble control para asegurar movimiento estrictamente en plano XZ

        //Distancia de parada (Umbral de tolerancia): si está muy cerca del destino, frena para evitar parpadeos
        if (direction.magnitude < 0.2f)
        {
            return;
        }

        //Calcula el paso de movimiento usando velocidad constante, normalizando el vector dirección y aplicando DeltaTime
        Vector3 newPosition = currentPosition + direction.normalized * blackboard.moveSpeed * Time.deltaTime;
        newPosition.y = currentPosition.y;

        transform.position = newPosition;

        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);

        //Interpola de forma esférica (Slerp) la rotación actual hacia la deseada para un giro realista
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            blackboard.rotationSpeed * Time.deltaTime
        );
    }

    //Método para mirar al jugador
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

    //Método para centralizar la llamada al componente animator de Unity de forma segura para evitar excepciones de referencia nula
    private void PlayAnimation(string animationName)
    {
        if (blackboard.animator != null)
        {
            blackboard.animator.Play(animationName);
        }
    }
}