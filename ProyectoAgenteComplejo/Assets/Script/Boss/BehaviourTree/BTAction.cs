public class BTAction : BTTask
{
    private System.Action action;

    public BTAction(System.Action action)
    {
        this.action = action;
    }

    public override bool Run()
    {
        action();
        return true;
    }
}