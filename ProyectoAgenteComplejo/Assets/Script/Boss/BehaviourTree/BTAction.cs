using UnityEngine;

//Nodo de tipo Acción (Hoja)
//Actúa como una hoja en el Árbol de Comportamiento encargada de ejecutar comandos directos sobre el agente
//Utiliza delegados de C# para envolver cualquier método que devuelva void, permitiendo reutilizar
//este único script para ejecutar todas las animaciones y movimientos físicos del Jefe
public class BTAction : BTTask
{
    //Almacena la referencia a la acción/método físico que se quiere ejecutar (delegado que retorna void)
    private System.Action action;

    //Constructor del nodo de acción que almacena el método de comportamiento deseado en el atributo action
    public BTAction(System.Action action)
    {
        this.action = action;
    }

    //Ejecuta la acción pasada en el constructor y reporta el resultado al nodo superior
    //Retorna siempre true para indicar que la acción se ha mandado a ejecutar con éxito
    public override bool Run()
    {
        if (action == null)
        {
            return false;
        }

        //Invoca el método dinámicamente (mueve al jefe, reproduce animación, etc...)
        action();
        return true;
    }
}