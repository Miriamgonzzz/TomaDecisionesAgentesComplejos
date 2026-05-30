using System.Collections.Generic;

public class BTSequence : BTTask
{
    private List<BTTask> children;

    public BTSequence(List<BTTask> children)
    {
        this.children = children;
    }

    public override bool Run()
    {
        foreach (BTTask child in children)
        {
            if (!child.Run())
                return false;
        }

        return true;
    }
}
