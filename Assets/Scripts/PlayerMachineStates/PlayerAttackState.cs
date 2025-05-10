// PlayerAttackState.cs
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float _attackTimer;

    public PlayerAttackState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        // Debug.Log("Player entered Light Attack State");
        _attackTimer = _ctx.AttackDuration; // Usa la duración del ataque ligero
        _ctx.Anim?.SetTrigger("Attack");    // Dispara el trigger "Attack" (ligero)
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