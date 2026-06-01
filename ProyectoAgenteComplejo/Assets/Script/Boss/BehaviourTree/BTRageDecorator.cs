
//Nodo de tipo Decorator para el control del Modo furia
//Los decoradores actúan como filtros condicionales de un único nodo hijo
//Este decorador evalúa si el Jefe cumple con los requisitos de salud para entrar en furia
//SI es así, permite la ejecución de su hijo (un nodo de acción para activar la furia),
//en caso contrario, bloquea el paso de ejecución
public class BTRageDecorator : BTDecorator
{
    private BossBlackboard blackboard;

    //Constructor
    public BTRageDecorator(BossBlackboard blackboard, BTTask child) : base(child)
    {
        this.blackboard = blackboard;
    }

    //Ejecuta la condición de filtro y, si se supera, delega la ejecución en su nodo hijo
    public override bool Run()
    {
        if (!blackboard.ShouldEnterRage())
        {
            return false;
        }

        return child.Run();
    }
}