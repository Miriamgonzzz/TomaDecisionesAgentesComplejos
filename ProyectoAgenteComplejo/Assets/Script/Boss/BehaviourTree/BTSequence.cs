using System.Collections.Generic;

//Nodo Sequence
//Funciona de manera similar a un operador lógico AND. Ejecuta a sus hijos en orden secuencial
//Si cualquiera de los hijos falla (retorna false), la secuencia se aborta inmediatamente y devuelve false
//Solo devuelve true si todos sus hijos se ejecutan con éxito, de ahí el nombre de secuencia
public class BTSequence : BTTask
{
    //Lista ordenada de nodos hijos que componen esta secuencia
    private List<BTTask> children;

    //Constructor que inicializa el nodo secuenciador con su lista de tareas hijas
    public BTSequence(List<BTTask> children)
    {
        this.children = children;
    }

    //Ejecuta de forma secuencial cada nodo hijo de la lista.
    //Si falla un hijo, retorna false, solo retorna true si todos los hijos van bien
    public override bool Run()
    {
        foreach (BTTask child in children)
        {
            if (!child.Run())
            {
                return false;
            }
        }
        return true; 
    }
}