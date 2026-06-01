using UnityEngine;

//Clase base abstracta que define el contrato fundamental para todos los nodos del Árbol de Comportamiento
//Cualquier tarea, condición o selector que forme parte del árbol deberá heredar de esta clase e implementar su lógica
public abstract class BTTask
{
    //Método central de ejecución del nodo. Se invoca en cada evaluación del árbol para procesar la tarea
    //Retorna true si la tarea se ejecutó con éxito
    //Retorna false si la tarea falló o no cumplió los requisitos
    public abstract bool Run();
}