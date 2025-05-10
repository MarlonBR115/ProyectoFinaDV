// PlayerStateMachine.cs
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class PlayerStateMachine : MonoBehaviour
{
    // --- REFERENCIAS A COMPONENTES ---
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public BoxCollider2D Coll { get; private set; }

    // --- CONFIGURACIÓN DE MOVIMIENTO ---
    [Header("Movement Stats")]
    public float MoveSpeed = 7f;
    public float JumpForce = 14f;
    [SerializeField] private LayerMask _groundLayer;

    // --- CONFIGURACIÓN DE ATAQUE ---
    [Header("Attack Stats")]
    public float AttackDuration = 0.3f;
    public float HeavyAttackDuration = 0.7f;
    public float TimeToHoldForHeavyAttack = 0.25f;

    [Header("Combat Hitboxes")]
    public GameObject attackHitboxObject;
    private List<Collider2D> _alreadyHitEnemiesInSwing;

    // --- CONFIGURACIÓN DE PARRY ---
    [Header("Parry Mechanics")]
    public GameObject ParryHitboxObject;
    public PlayerParryHitbox ParryHitboxScript { get; private set; }
    public float ParryWindowDuration = 0.25f;
    public float ParryCooldown = 1.0f;
    public float LastParryTime { get; set; } = -100f;

    // --- ESTADÍSTICAS DEL JUGADOR ---
    [Header("Player Stats")]
    public int maxHealth = 100;
    private int _currentHealth;
    public bool IsDead { get; private set; } = false;
    public float hurtStateDuration = 0.4f; // NUEVO: Duración del estado "Hurt" (stun)

    // --- EFECTOS DE FEEDBACK ---
    [Header("Feedback Effects")]
    public float parrySlowMotionDuration = 0.2f;
    public float parrySlowMotionScale = 0.3f;
    public float playerHitShakeDuration = 0.15f;
    public float playerHitShakeMagnitude = 0.1f;

    private CameraShake _cameraShakeInstance;

    // --- MÁQUINA DE ESTADOS ---
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    // --- INPUTS Y ESTADO INTERNO ---
    public float HorizontalInput { get; private set; }
    public bool IsFacingRight { get; private set; } = true;
    public bool JumpInputPressed { get; private set; }
    public bool AttackInputPressed { get; private set; }
    public bool AttackInputHeld { get; private set; }
    public bool AttackInputReleased { get; private set; }
    public bool ParryInputPressed { get; private set; }
    public float CurrentAttackHoldTime { get; set; }


    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        Coll = GetComponent<BoxCollider2D>();

        if (Rb == null) Debug.LogError("PLAYER Awake(): Rigidbody2D no encontrado. ¡Asegúrate de añadirlo!");
        if (Anim == null) Debug.LogError("PLAYER Awake(): Animator no encontrado. ¡Asegúrate de añadirlo y configurarlo!");
        if (Coll == null) Debug.LogError("PLAYER Awake(): Collider2D principal no encontrado.");

        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();

        _currentHealth = maxHealth;
        IsDead = false;

        LastParryTime = -ParryCooldown;
        if (ParryHitboxObject != null)
        {
            ParryHitboxScript = ParryHitboxObject.GetComponent<PlayerParryHitbox>();
            if (ParryHitboxScript == null)
            {
                Debug.LogError("PLAYER Awake(): PlayerParryHitbox script no encontrado en ParryHitboxObject!");
            }
            ParryHitboxObject.SetActive(false);
        }
        else
        {
            Debug.LogError("PLAYER Awake(): ParryHitboxObject no ha sido asignado en el Inspector!");
        }

        _alreadyHitEnemiesInSwing = new List<Collider2D>();
        if (attackHitboxObject == null)
        {
            Debug.LogError("PLAYER Awake(): attackHitboxObject no asignado en el Inspector!");
        }
        else
        {
            attackHitboxObject.SetActive(false);
        }

        if (Camera.main != null)
        {
            _cameraShakeInstance = Camera.main.GetComponent<CameraShake>();
            if (_cameraShakeInstance == null)
            {
                Debug.LogWarning("PLAYER Awake(): No se encontró el script CameraShake en la cámara principal (Camera.main). El efecto de shake no funcionará. Asegúrate de añadir CameraShake.cs a tu Main Camera.");
            }
        }
        else
        {
            Debug.LogWarning("PLAYER Awake(): No se encontró la cámara principal (Camera.main). El efecto de shake no funcionará.");
        }
        
        if (_currentState != null)
        {
            _currentState.EnterState();
        }
        else
        {
            Debug.LogError("PLAYER Awake(): _currentState es null después de la inicialización.");
        }
    }

    void Update()
    {
        if (IsDead) return; // Si el jugador está muerto, no procesar updates normales

        // Solo procesar inputs si el estado actual lo permite (ej. no si está en Hurt o Dead)
        // O podrías poner esta lógica dentro de cada estado si algunos permiten ciertos inputs y otros no.
        // Por ahora, si está en Hurt, su propio Update está vacío y CheckSwitchStates solo mira el timer.
        if (!(_currentState is PlayerHurtState)) // No leer inputs si está en Hurt
        {
             HorizontalInput = Input.GetAxisRaw("Horizontal");
             JumpInputPressed = Input.GetButtonDown("Jump");
             AttackInputPressed = Input.GetButtonDown("Fire1");
             AttackInputHeld = Input.GetButton("Fire1");
             AttackInputReleased = Input.GetButtonUp("Fire1");
             ParryInputPressed = Input.GetButtonDown("Fire2");
        } else {
             // Resetear inputs para que no se queden "pegados" al salir del estado Hurt
             HorizontalInput = 0f;
             JumpInputPressed = false;
             AttackInputPressed = false;
             AttackInputHeld = false; // Importante si el botón se quedó presionado al entrar a Hurt
             AttackInputReleased = false; 
             ParryInputPressed = false;
        }


        _currentState?.UpdateState();
        _currentState?.CheckSwitchStates();
    }

    void FixedUpdate()
    {
        if (IsDead) return;
        _currentState?.FixedUpdateState();
    }

    public void ChangeState(PlayerBaseState newState)
    {
        // No permitir cambiar de estado si está muerto, a menos que el nuevo estado sea PlayerDeadState
        if (IsDead && !(newState is PlayerDeadState)) return; 
        
        _currentState?.ExitState();
        _currentState = newState;
        _currentState?.EnterState();
    }

    public void Flip()
    {
        if (IsDead || (_currentState is PlayerHurtState)) return; // No girar si está muerto o herido
        IsFacingRight = !IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public bool IsGrounded()
    {
        if (Coll == null) return false;
        float extraHeightText = 0.1f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(Coll.bounds.center,
                                                    new Vector2(Coll.bounds.size.x * 0.9f, Coll.bounds.size.y),
                                                    0f, Vector2.down, extraHeightText, _groundLayer);
        return raycastHit.collider != null;
    }

    public bool CanParry()
    {
        if (IsDead || (_currentState is PlayerHurtState)) return false;
        return Time.time >= LastParryTime + ParryCooldown;
    }

    public void NotifyParrySuccess()
    {
        if (IsDead) return;
        if (_currentState is PlayerParryState parryState)
        {
            parryState.RegisterSuccessfulParry();
            if (parrySlowMotionDuration > 0 && parrySlowMotionScale < 1.0f && parrySlowMotionScale > 0)
            {
                StartCoroutine(DoParrySlowMotionCoroutine());
            }
        }
    }

    private IEnumerator DoParrySlowMotionCoroutine()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = parrySlowMotionScale; 

        yield return new WaitForSecondsRealtime(parrySlowMotionDuration); 

        Time.timeScale = originalTimeScale; 
    }


    public float GetAnimationClipLength(string clipName)
    {
        if (Anim == null || Anim.runtimeAnimatorController == null) return 0f;
        
        foreach (AnimationClip clip in Anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length / (Anim.speed == 0 ? 1f : Anim.speed);
            }
        }
        Debug.LogWarning($"PLAYER GetAnimationClipLength(): Animation clip '{clipName}' no encontrado. Verifica el nombre exacto en tu Animator Controller y el archivo .anim.");
        return 0f;
    }

    public void EnableAttackHitbox()
    {
        if (IsDead || (_currentState is PlayerHurtState)) return; // No activar hitbox si muerto o herido
        if (attackHitboxObject != null)
        {
            _alreadyHitEnemiesInSwing.Clear();
            attackHitboxObject.SetActive(true);
        }
    }

    public void DisableAttackHitbox()
    {
        // Desactivar siempre, incluso si está muerto (por si el evento de anim llega tarde)
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
        }
    }

    public bool CanHitTarget(Collider2D targetCollider)
    {
        if (IsDead || (_currentState is PlayerHurtState)) return false;
        if (_alreadyHitEnemiesInSwing.Contains(targetCollider))
        {
            return false;
        }
        else
        {
            _alreadyHitEnemiesInSwing.Add(targetCollider);
            return true;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead || (_currentState is PlayerHurtState && _currentState.GetType() != typeof(PlayerHurtState))) // Evitar re-entrar a Hurt si ya está en Hurt por este mismo golpe
        {
             // Si ya está en Hurt por este golpe, o muerto, no procesar más daño de este mismo evento de golpe
             // Esto puede necesitar una lógica de i-frames más robusta si los ataques enemigos pueden golpear muy rápido
             return;
        }


        _currentHealth -= damageAmount;
        Debug.Log($"PLAYER: Recibió {damageAmount} de daño. Vida restante: {_currentHealth}");

        Anim?.SetTrigger("Hurt"); // Asegúrate que "Hurt" exista como Trigger en tu Animator

        if (_cameraShakeInstance != null)
        {
            _cameraShakeInstance.StartShake(playerHitShakeDuration, playerHitShakeMagnitude);
        }
        else
        {
            Debug.LogWarning("PLAYER TakeDamage(): _cameraShakeInstance es null. No se pudo activar el shake.");
        }

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die(); // Die se encargará de cambiar al estado Dead
        }
        else
        {
            // Solo cambiar a HurtState si no estamos ya procesando la muerte
            if (!IsDead && _states != null)
            {
                ChangeState(_states.Hurt()); // <<<--- CAMBIAR AL ESTADO HURT
            }
        }
    }

    void Die()
    {
        if (IsDead) return;

        IsDead = true;
        Debug.Log("PLAYER: Ha muerto.");
        Anim?.SetTrigger("PlayerDie"); 

        if (Rb != null)
        {
            Rb.linearVelocity = Vector2.zero;
            Rb.bodyType = RigidbodyType2D.Kinematic; 
        }
        else
        {
            Debug.LogWarning("PLAYER Die(): Rigidbody2D (Rb) es null.");
        }

        if (_states != null)
        {
            ChangeState(_states.Dead());
        }
        else
        {
            Debug.LogError("PLAYER Die(): _states (PlayerStateFactory) es null. No se pudo cambiar a DeadState.");
            this.enabled = false; 
        }
    }
}