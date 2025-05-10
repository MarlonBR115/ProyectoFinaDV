// PlayerIdleState.cs
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        _ctx.Rb.linearVelocity = new Vector2(0, _ctx.Rb.linearVelocity.y);
        _ctx.Anim?.SetBool("IsMoving", false);
        _ctx.Anim?.SetBool("IsJumping", false);
        _ctx.Anim?.SetBool("Parry", false);
    }

    public override void UpdateState() { }
    public override void FixedUpdateState() { }
    public override void ExitState() { }

    public override void CheckSwitchStates()
    {
        if (_ctx.AttackInputPressed) // Si se presiona Fire1
        {
            _ctx.ChangeState(_factory.AttackPrimed()); // Iniciar preparación de ataque
            return;
        }

        if (_ctx.ParryInputPressed && _ctx.CanParry()) _ctx.ChangeState(_factory.Parry());
        else if (_ctx.JumpInputPressed && _ctx.IsGrounded()) _ctx.ChangeState(_factory.Jump());
        else if (_ctx.HorizontalInput != 0) _ctx.ChangeState(_factory.Move());
    }
    public override void InitializeSubState() { }
}