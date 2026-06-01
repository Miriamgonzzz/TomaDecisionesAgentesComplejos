//Clase base abstracta para todos los nodos de tipo Decorator en el Árbol de Comportamiento
public abstract class BTDecorator : BTTask
{
    //El nodo hijo único que este decorador se encarga de supervisar y ejecutar
    //Se declara como 'protected' para que solo las clases derivadas puedan acceder a él directamente
    protected BTTask child;

    //Constructor
    public BTDecorator(BTTask child)
    {
        this.child = child;
    }
}