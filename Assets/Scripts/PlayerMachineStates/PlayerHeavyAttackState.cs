// PlayerHeavyAttackState.cs
using UnityEngine;

public class PlayerHeavyAttackState : PlayerBaseState
{
    private float _attackTimer;

    public PlayerHeavyAttackState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        // Debug.Log("Player entered Heavy Attack State");
        _attackTimer = _ctx.HeavyAttackDuration; // Usa la duración del ataque pesado
        _ctx.Anim?.SetTrigger("HeavyAttack");    // Dispara el trigger "HeavyAttack"
        _ctx.Rb.linearVelocity = new Vector2(0, _ctx.Rb.linearVelocity.y);
    }

    public override void UpdateState()
    {
        _attackTimer -= Time.deltaTime;
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }

    public override void CheckSwitchStates()
    {
        if (_attackTimer <= 0)
        {
            if (_ctx.IsGrounded())
            {
                if (_ctx.HorizontalInput == 0) _ctx.ChangeState(_factory.Idle());
                else _ctx.ChangeState(_factory.Move());
            }
            else
            {
                _ctx.ChangeState(_factory.Jump());
            }
        }
    }
    public override void InitializeSubState() { }
}