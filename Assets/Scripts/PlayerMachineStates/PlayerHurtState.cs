// PlayerHurtState.cs
using UnityEngine;

public class PlayerHurtState : PlayerBaseState
{
    private float _hurtTimer;

    public PlayerHurtState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        // Debug.Log("PLAYER FSM: Entrando a PlayerHurtState.");
        _hurtTimer = _ctx.hurtStateDuration; // Usar la duración definida en PlayerStateMachine

        // El trigger "Hurt" ya se llamó en PlayerStateMachine.TakeDamage()
        // Asegurarse de que otros parámetros de animación de movimiento estén desactivados
        _ctx.Anim?.SetBool("IsMoving", false);
        _ctx.Anim?.SetBool("IsJumping", false);
        _ctx.Anim?.SetBool("Parry", false); // Por si acaso estaba en parry

        // Detener el movimiento inmediatamente
        if (_ctx.Rb != null)
        {
            _ctx.Rb.linearVelocity = Vector2.zero;
        }

        // Desactivar hitboxes por si estaba atacando o haciendo parry
        _ctx.DisableAttackHitbox(); // Llama al método que ya tienes
        if (_ctx.ParryHitboxObject != null)
        {
            _ctx.ParryHitboxObject.SetActive(false);
        }
    }

    public override void UpdateState()
    {
        if (_hurtTimer > 0)
        {
            _hurtTimer -= Time.deltaTime;
        }
    }

    public override void FixedUpdateState()
    {
        // Mientras está en "hurt", podría ser bueno asegurar que no se mueva por fuerzas externas
        // o si el Rigidbody no es cinemático y la animación de hurt no lo controla.
        if (_ctx.Rb != null && _ctx.Rb.bodyType == RigidbodyType2D.Dynamic) // Si es dinámico
        {
             // Podrías aplicar una pequeña fuerza de frenado o simplemente mantener la velocidad en cero
             // si la animación de hurt no implica movimiento.
             // _ctx.Rb.velocity = Vector2.zero; // Comenta o descomenta según el feeling deseado
        }
    }

    public override void ExitState()
    {
        // Debug.Log("PLAYER FSM: Saliendo de PlayerHurtState.");
        // No es necesario hacer mucho aquí, el siguiente estado (Idle, Jump)
        // se encargará de sus propias animaciones y física al entrar.
    }

    public override void CheckSwitchStates()
    {
        if (_hurtTimer <= 0)
        {
            // Cuando el tiempo de "hurt" termina, decidir a qué estado volver.
            // Usualmente Idle si está en el suelo, o Jump/Fall si está en el aire.
            if (_ctx.IsGrounded())
            {
                _ctx.ChangeState(_factory.Idle());
            }
            else
            {
                _ctx.ChangeState(_factory.Jump()); // El estado Jump maneja la caída
            }
        }
    }

    public override void InitializeSubState() { }
}