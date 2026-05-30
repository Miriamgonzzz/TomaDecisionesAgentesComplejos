public abstract class BTDecorator : BTTask
{
    protected BTTask child;

    public BTDecorator(BTTask child)
    {
        this.child = child;
    }
}