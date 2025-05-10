public class EnemyStateFactory
{
    EnemyController _context;

    public EnemyStateFactory(EnemyController currentContext)
    {
        _context = currentContext;
    }

    public EnemyBaseState Idle() { return new EnemyIdleState(_context, this); }
    public EnemyBaseState Attack() { return new EnemyAttackState(_context, this); }
    public EnemyBaseState Stunned() { return new EnemyStunnedState(_context, this); }
}