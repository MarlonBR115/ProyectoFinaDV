using UnityEngine;

public class EnemyStunnedState : EnemyBaseState
{
    private float _stunTimer;

    public EnemyStunnedState(EnemyController currentContext, EnemyStateFactory enemyStateFactory)
        : base(currentContext, enemyStateFactory) { }

    public override void EnterState()
    {
        _stunTimer = _ctx.StunDuration;
        _ctx.Anim?.SetBool("IsStunned", true); // Activar animación de stun
        _ctx.Rb.linearVelocity = Vector2.zero;

        // Si el enemigo estaba atacando y fue parreado, asegurarse que su hitbox de ataque se desactive
        if (_ctx.IsAttacking && _ctx.AttackHitbox != null)
        {
            _ctx.AttackHitbox.SetActive(false);
            _ctx.IsAttacking = false; // Marcar que ya no está en el proceso de ataque
        }
    }

    public override void UpdateState()
    {
        _stunTimer -= Time.deltaTime;
    }

    public override void FixedUpdateState() { }

    public override void ExitState()
    {
        _ctx.Anim?.SetBool("IsStunned", false); // Desactivar animación de stun
    }

    public override void CheckSwitchStates()
    {
        if (_stunTimer <= 0)
        {
            _ctx.ChangeState(_factory.Idle());
        }
    }
}