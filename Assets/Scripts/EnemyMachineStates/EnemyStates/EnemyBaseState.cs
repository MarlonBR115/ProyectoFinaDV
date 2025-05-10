public abstract class EnemyBaseState
{
    protected EnemyController _ctx;
    protected EnemyStateFactory _factory;

    public EnemyBaseState(EnemyController currentContext, EnemyStateFactory enemyStateFactory)
    {
        _ctx = currentContext;
        _factory = enemyStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
}