using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    public PlayerDeadState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        Debug.Log("PLAYER FSM: Entrando a PlayerDeadState.");

        _ctx.Anim?.SetBool("IsMoving", false);
        _ctx.Anim?.SetBool("IsJumping", false);
        _ctx.Anim?.SetBool("Parry", false);
        // El trigger "PlayerDie" ya fue llamado en PlayerStateMachine.Die()

        // La física (detener velocidad, hacerlo cinemático) también ya se maneja en PlayerStateMachine.Die()
        // Pero como redundancia o si se llama a EnterState por otra vía:
        if (_ctx.Rb != null)
        {
            _ctx.Rb.linearVelocity = Vector2.zero;
            // Asegurarse de que sea Kinematic si no lo es ya
            if (_ctx.Rb.bodyType != RigidbodyType2D.Kinematic) 
            {
                _ctx.Rb.bodyType = RigidbodyType2D.Kinematic; // <<<--- CAMBIO IMPORTANTE AQUÍ
            }
        }

        // Desactivar hitboxes
        if (_ctx.attackHitboxObject != null)
            _ctx.attackHitboxObject.SetActive(false);
        if (_ctx.ParryHitboxObject != null)
            _ctx.ParryHitboxObject.SetActive(false);

        // Opcional: Desactivar el collider principal
        // if (_ctx.Coll != null)
        //     _ctx.Coll.enabled = false;
    }

    public override void UpdateState()
    {
        // El jugador está muerto. La animación de muerte debería estar reproduciéndose.
        // No hay lógica de update aquí usualmente.
    }

    public override void FixedUpdateState()
    {
        // Asegurarse de que no haya movimiento si el Rigidbody no es Kinematic por alguna razón
        if (_ctx.Rb != null && _ctx.Rb.bodyType != RigidbodyType2D.Kinematic) // <<<--- CAMBIO IMPORTANTE AQUÍ
        {
            _ctx.Rb.linearVelocity = Vector2.zero;
        }
    }

    public override void ExitState()
    {
        // Normalmente no se sale de este estado a menos que haya una mecánica de revivir.
        // Si se revive, habría que restaurar el bodyType del Rigidbody a Dynamic.
        Debug.Log("PLAYER FSM: Saliendo de PlayerDeadState (inusual).");
        // if (_ctx.Rb != null)
        // {
        //     _ctx.Rb.bodyType = RigidbodyType2D.Dynamic;
        // }
    }

    public override void CheckSwitchStates()
    {
        // No se cambia a otros estados desde aquí.
    }

    public override void InitializeSubState() { }
}