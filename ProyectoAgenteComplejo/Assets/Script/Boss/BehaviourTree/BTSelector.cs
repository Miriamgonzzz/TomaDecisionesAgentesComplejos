using System.Collections.Generic;

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
                return true;
        }

        return false;
    }
}