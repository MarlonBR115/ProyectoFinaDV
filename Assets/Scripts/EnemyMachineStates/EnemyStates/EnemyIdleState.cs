using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    private float _idleTimer;
    private float _timeToWaitBeforeAttack;

    public EnemyIdleState(EnemyController currentContext, EnemyStateFactory enemyStateFactory)
        : base(currentContext, enemyStateFactory) { }

    public override void EnterState()
    {
        _timeToWaitBeforeAttack = Random.Range(_ctx.MinIdleTime, _ctx.MaxIdleTime);
        _idleTimer = 0f;
        _ctx.Anim?.SetBool("IsStunned", false);
        // Si tienes un trigger para forzar la animación de Idle:
        // _ctx.Anim?.SetTrigger("GoToIdle");
        // O si "Idle" es el estado por defecto al que vuelve el Animator, esto es suficiente.
    }

    public override void UpdateState()
    {
        _idleTimer += Time.deltaTime;
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }

    public override void CheckSwitchStates()
    {
        if (_idleTimer >= _timeToWaitBeforeAttack && _ctx.PlayerReference != null)
        {
            _ctx.ChangeState(_factory.Attack());
        }
    }
}