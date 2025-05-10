using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private float _stateTimer; // Timer para la duración total del estado de ataque

    public EnemyAttackState(EnemyController currentContext, EnemyStateFactory enemyStateFactory)
        : base(currentContext, enemyStateFactory) { }

    public override void EnterState()
    {
        _ctx.IsAttacking = true; // Marcar que el enemigo está en su lógica de ataque
        // La duración total del estado se basa en los tiempos definidos en EnemyController.
        // La activación/desactivación del hitbox real la harán los Animation Events.
        _stateTimer = _ctx.AttackWindUpTime + _ctx.AttackActiveTime + _ctx.AttackCooldownTime;

        _ctx.TriggerAnimation("Attack"); // Disparar el trigger "Attack" en el Animator del Enemigo

        // Orientar al jugador (opcional)
        if (_ctx.PlayerReference != null)
        {
            if (_ctx.PlayerReference.transform.position.x < _ctx.transform.position.x && _ctx.IsFacingRight)
                _ctx.Flip();
            else if (_ctx.PlayerReference.transform.position.x > _ctx.transform.position.x && !_ctx.IsFacingRight)
                _ctx.Flip();
        }
    }

    public override void UpdateState()
    {
        if (_stateTimer > 0)
        {
            _stateTimer -= Time.deltaTime;
        }
    }

    public override void FixedUpdateState() { }

    public override void ExitState()
    {
        _ctx.IsAttacking = false; // Marcar que el enemigo ya no está en su lógica de ataque
        // Como medida de seguridad, asegurarse de que el hitbox esté desactivado al salir del estado,
        // aunque el Animation Event debería haberlo hecho.
        _ctx.DisableEnemyAttackHitbox();
    }

    public override void CheckSwitchStates()
    {
        if (_stateTimer <= 0)
        {
            _ctx.ChangeState(_factory.Idle());
        }
    }
}