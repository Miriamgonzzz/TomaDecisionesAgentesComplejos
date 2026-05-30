public class BTCondition : BTTask
{
    private System.Func<bool> condition;

    public BTCondition(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override bool Run()
    {
        return condition();
    }
}