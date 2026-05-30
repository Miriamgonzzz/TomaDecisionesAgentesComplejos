public class BTRageDecorator : BTDecorator
{
    private BossBlackboard blackboard;

    public BTRageDecorator(BossBlackboard blackboard, BTTask child) : base(child)
    {
        this.blackboard = blackboard;
    }

    public override bool Run()
    {
        if (!blackboard.ShouldEnterRage())
            return false;

        return child.Run();
    }
}