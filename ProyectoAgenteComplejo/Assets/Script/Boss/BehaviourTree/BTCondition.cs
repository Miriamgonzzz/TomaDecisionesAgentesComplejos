using UnityEngine;

//Nodo de tipo Condición (Hoja)
//Utiliza delegados de C# para evaluar cualquier función externa que devuelva un booleano,
//lo que permite reutilizar este único script para múltiples comprobaciones lógicas del Jefe y evitar multitud de clases
public class BTCondition : BTTask
{
    //Almacena la referencia a la función que se quiere evaluar (delegado que retorna un booleano)
    private System.Func<bool> condition;

    //Constructor del nodo condicional que recibe la función de comprobación deseada y la almacena en el atributo
    public BTCondition(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    //Ejecuta la evaluación de la función almacenada en el atributo, retornando true o false para
    //indicar si se puede o no ejecutar
    public override bool Run()
    {
        if (condition == null)
        {
            return false;
        }
        return condition();
    }
}