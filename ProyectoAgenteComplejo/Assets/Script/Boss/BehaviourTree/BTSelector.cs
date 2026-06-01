using System.Collections.Generic;

//Nodo compuesto de tipo Selector
//Funciona de manera similar a un operador lógico OR. Evalúa a sus hijos en orden secuencial
//buscando el primer comportamiento que se ejecute con éxito.
//Si cualquiera de los hijos tiene éxito (retorna true), el selector se detiene inmediatamente y devuelve true.
//Solo devuelve false si TODOS sus hijos fallaron
//La lógica es casi igual que la del Sequence
public class BTSelector : BTTask
{
    private List<BTTask> children;

    public BTSelector(List<BTTask> children)
    {
        this.children = children;
    }

    public override bool Run()
    {
        
        foreach (BTTask child in children)
        {
            
            if (child.Run())
            {
                return true;
            }
        }
        return false; 
    }
}