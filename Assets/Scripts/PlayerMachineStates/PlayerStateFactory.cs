// PlayerStateFactory.cs
public class PlayerStateFactory
{
    PlayerStateMachine _context;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    public PlayerBaseState Idle() { return new PlayerIdleState(_context, this); }
    public PlayerBaseState Move() { return new PlayerMoveState(_context, this); }
    public PlayerBaseState Jump() { return new PlayerJumpState(_context, this); }
    public PlayerBaseState AttackPrimed() { return new PlayerAttackPrimedState(_context, this); }
    public PlayerBaseState Attack() { return new PlayerAttackState(_context, this); }
    public PlayerBaseState HeavyAttack() { return new PlayerHeavyAttackState(_context, this); }
    public PlayerBaseState Parry() { return new PlayerParryState(_context, this); }
    public PlayerBaseState Dead() { return new PlayerDeadState(_context, this); }
    public PlayerBaseState Hurt() { return new PlayerHurtState(_context, this); } // <<<--- AÑADIR ESTA LÍNEA
}