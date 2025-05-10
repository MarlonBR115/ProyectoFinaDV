// PlayerParryState.cs
using UnityEngine;

public class PlayerParryState : PlayerBaseState
{
    private float _parryWindowTimer;    // Cuánto dura la ventana activa para hacer parry
    private float _timeInState;         // Cuánto tiempo llevamos en este estado en total
    private bool _isParrySuccessful;
    private float _fullAnimationDuration; // Duración completa de la animación de parry

    public PlayerParryState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        // Debug.Log("PLAYER: Entering Parry State");
        _isParrySuccessful = false;
        _parryWindowTimer = _ctx.ParryWindowDuration;
        _timeInState = 0f;

        // Obtener la duración de la animación de parry.
        // Asegúrate de que el nombre "Player_Parry" coincida con el nombre de tu Animation Clip.
        _fullAnimationDuration = _ctx.GetAnimationClipLength("PlayerParry");
        if (_fullAnimationDuration == 0f)
        {
            Debug.LogWarning("PlayerParry animation clip duration is 0 or not found. Parry success might not behave as expected.");
            _fullAnimationDuration = _ctx.ParryWindowDuration * 2; // Fallback si no se encuentra
        }


        _ctx.Anim?.SetBool("Parry", true); // Iniciar animación de parry

        if (_ctx.ParryHitboxObject != null)
        {
            _ctx.ParryHitboxObject.SetActive(true);
            _ctx.ParryHitboxScript?.ResetCollider(); // Asegurarse de que el collider esté activo
        }
        _ctx.Rb.linearVelocity = new Vector2(0, _ctx.Rb.linearVelocity.y);
    }

    public override void UpdateState()
    {
        _timeInState += Time.deltaTime;

        if (!_isParrySuccessful && _parryWindowTimer > 0)
        {
            _parryWindowTimer -= Time.deltaTime;
            if (_parryWindowTimer <= 0)
            {
                // La ventana de parry terminó sin éxito, desactivar hitbox si aún no lo está
                if (_ctx.ParryHitboxObject != null && _ctx.ParryHitboxObject.activeSelf)
                {
                    // No desactivamos el GameObject completo aquí, solo el collider si es necesario,
                    // o simplemente dejamos que CheckSwitchStates lo maneje.
                    // Por ahora, CheckSwitchStates decidirá.
                }
            }
        }
    }

    public void RegisterSuccessfulParry()
    {
        if (!_isParrySuccessful) // Asegurarse de que solo se registre una vez
        {
            _isParrySuccessful = true;
            // El PlayerParryHitbox ya desactiva su propio collider al tener éxito.
            // No es necesario desactivar el GameObject ParryBoxObject aquí,
            // se hará en ExitState.
            // Debug.Log("PLAYER: Parry success registered in state.");
        }
    }

    public override void FixedUpdateState() { }

    public override void ExitState()
    {
        // Debug.Log("PLAYER: Exiting Parry State. Success: " + _isParrySuccessful);
        _ctx.Anim?.SetBool("Parry", false); // Terminar animación de parry

        if (_ctx.ParryHitboxObject != null)
        {
            _ctx.ParryHitboxObject.SetActive(false);
        }
    }

    public override void CheckSwitchStates()
    {
        if (_isParrySuccessful)
        {
            // Si el parry fue exitoso, esperamos que la animación completa se reproduzca.
            // El Animator se encargará de la transición de salida usando "Has Exit Time"
            // cuando IsParrying se ponga a false en ExitState.
            // Solo necesitamos decidir cuándo el *estado de código* debe terminar.
            if (_timeInState >= _fullAnimationDuration)
            {
                // Debug.Log("PLAYER: Parry successful, animation time ended. Switching to Idle.");
                _ctx.ChangeState(_factory.Idle());
            }
            // Si no, permanecemos en este estado para que la animación continúe.
        }
        else // Parry no fue (aún) exitoso
        {
            // Si la ventana de parry activa ha terminado
            if (_parryWindowTimer <= 0)
            {
                // Debug.Log("PLAYER: Parry window ended (miss). Switching to Idle.");
                _ctx.ChangeState(_factory.Idle()); // Esto llamará a ExitState, que pondrá IsParrying a false
                                                   // y el Animator usará la transición de "corte"
            }
            // Si no, permanecemos en este estado, la ventana de parry sigue activa.
        }
    }
    public override void InitializeSubState() { }
}