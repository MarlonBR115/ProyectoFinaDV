// PlayerMoveState.cs
using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        _ctx.Anim?.SetBool("IsMoving", true);
        _ctx.Anim?.SetBool("IsJumping", false);
        _ctx.Anim?.SetBool("Parry", false);
    }

    public override void UpdateState()
    {
        if (_ctx.HorizontalInput > 0 && !_ctx.IsFacingRight) _ctx.Flip();
        else if (_ctx.HorizontalInput < 0 && _ctx.IsFacingRight) _ctx.Flip();
    }

    public override void FixedUpdateState()
    {
        _ctx.Rb.linearVelocity = new Vector2(_ctx.HorizontalInput * _ctx.MoveSpeed, _ctx.Rb.linearVelocity.y);
    }

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
        else if (_ctx.HorizontalInput == 0 && _ctx.IsGrounded()) _ctx.ChangeState(_factory.Idle());
    }
    public override void InitializeSubState() { }
}