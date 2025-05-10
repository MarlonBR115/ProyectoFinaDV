// PlayerJumpState.cs
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private bool _jumpPerformed = false;

    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        _jumpPerformed = false;
        _ctx.Anim?.SetBool("IsJumping", true);
        _ctx.Anim?.SetBool("IsMoving", _ctx.HorizontalInput != 0);
        _ctx.Anim?.SetBool("Parry", false);
    }

    public override void UpdateState()
    {
        float horizontalInput = _ctx.HorizontalInput;
        _ctx.Rb.linearVelocity = new Vector2(horizontalInput * _ctx.MoveSpeed, _ctx.Rb.linearVelocity.y);
        _ctx.Anim?.SetBool("IsMoving", horizontalInput != 0);

        if (horizontalInput > 0 && !_ctx.IsFacingRight) _ctx.Flip();
        else if (horizontalInput < 0 && _ctx.IsFacingRight) _ctx.Flip();
    }

    public override void FixedUpdateState()
    {
        if (!_jumpPerformed)
        {
            _ctx.Rb.linearVelocity = new Vector2(_ctx.Rb.linearVelocity.x, 0);
            _ctx.Rb.AddForce(Vector2.up * _ctx.JumpForce, ForceMode2D.Impulse);
            _jumpPerformed = true;
        }
    }

    public override void ExitState() { }

    public override void CheckSwitchStates()
    {
        if (_ctx.Rb.linearVelocity.y <= 0.01f && _ctx.IsGrounded()) // Aterrizando
        {
            _ctx.Anim?.SetBool("IsJumping", false);
            if (_ctx.HorizontalInput == 0) _ctx.ChangeState(_factory.Idle());
            else _ctx.ChangeState(_factory.Move());
        }
        // Si se presiona el botón de ataque en el aire, es un ataque ligero directo
        else if (_ctx.AttackInputPressed)
        {
            _ctx.ChangeState(_factory.Attack());
        }
        else if (_ctx.ParryInputPressed && _ctx.CanParry())
        {
            _ctx.ChangeState(_factory.Parry());
        }
    }
    public override void InitializeSubState() { }
}