// PlayerAttackPrimedState.cs
using UnityEngine;

public class PlayerAttackPrimedState : PlayerBaseState
{
    public PlayerAttackPrimedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        // Debug.Log("Entering AttackPrimed State");
        _ctx.CurrentAttackHoldTime = 0f; // Reiniciar temporizador
        // Opcional: El jugador podría detenerse brevemente
        _ctx.Rb.linearVelocity = new Vector2(0f, _ctx.Rb.linearVelocity.y);
        _ctx.Anim?.SetBool("IsMoving", false); // Indicar que no se está moviendo para la animación
    }

    public override void UpdateState()
    {
        // Incrementar el tiempo de "mantener presionado" si el botón sigue presionado
        if (_ctx.AttackInputHeld)
        {
            _ctx.CurrentAttackHoldTime += Time.deltaTime;
        }
    }

    public override void FixedUpdateState() { }

    public override void ExitState()
    {
        // Debug.Log("Exiting AttackPrimed State. Hold time: " + _ctx.CurrentAttackHoldTime);
    }

    public override void CheckSwitchStates()
    {
        // PRIORIDAD 1: Interrupciones (Parry o Salto)
        if (_ctx.ParryInputPressed && _ctx.CanParry())
        {
            _ctx.ChangeState(_factory.Parry());
            return;
        }
        if (_ctx.JumpInputPressed && _ctx.IsGrounded())
        {
            _ctx.ChangeState(_factory.Jump());
            return;
        }

        // PRIORIDAD 2: Decisión de Ataque Pesado (si se mantiene el tiempo suficiente)
        if (_ctx.AttackInputHeld && _ctx.CurrentAttackHoldTime >= _ctx.TimeToHoldForHeavyAttack)
        {
            _ctx.ChangeState(_factory.HeavyAttack());
            return;
        }

        // PRIORIDAD 3: Decisión de Ataque Ligero (si se suelta el botón antes)
        if (_ctx.AttackInputReleased)
        {
            _ctx.ChangeState(_factory.Attack()); // Ataque ligero
            return;
        }

        // PRIORIDAD 4: Si el botón deja de estar presionado por otra razón (fallback)
        // O si el jugador empieza a moverse y queremos cancelar el "prime"
        if (!_ctx.AttackInputHeld)
        {
            // Si no se soltó este frame (AttackInputReleased fue false),
            // significa que el botón ya estaba arriba.
            // Consideramos esto como que la intención de ataque terminó.
            // Si se acumuló un poquito de tiempo, podría ser un ataque ligero, sino Idle.
            if (_ctx.CurrentAttackHoldTime > Time.deltaTime) // Un pequeño buffer para considerar un tap
            {
                _ctx.ChangeState(_factory.Attack());
            }
            else
            {
                _ctx.ChangeState(_factory.Idle());
            }
            return;
        }
        
        // Si el jugador intenta moverse mientras está en este estado de "preparación",
        // podríamos cancelarlo y volver a Move/Idle. Esto es una decisión de diseño.
        // Por ahora, el "prime" tiene prioridad si no se ha saltado/parry.
        // Si se quiere que el movimiento cancele:
        /*
        if (_ctx.HorizontalInput != 0)
        {
            _ctx.ChangeState(_factory.Move());
            return;
        }
        */

        // Si ninguna de las condiciones anteriores se cumple, significa que el botón sigue presionado
        // pero aún no se ha alcanzado el tiempo para el ataque pesado. Permanecemos en este estado.
    }
    public override void InitializeSubState() { }
}