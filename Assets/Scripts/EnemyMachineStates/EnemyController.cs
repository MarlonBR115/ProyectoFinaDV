using UnityEngine;
// using System.Collections.Generic; // No es necesario si solo rastreas un jugador

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public GameObject PlayerReference { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public GameObject AttackHitbox; // Asigna aquí el GameObject EnemyAttackHitbox (hijo del Enemy)

    [Header("Stats & Behavior")]
    public int maxHealth = 50;
    private int _currentHealth;
    public int attackDamage = 15; // Daño que hace el enemigo
    public float MinIdleTime = 2f;
    public float MaxIdleTime = 4f;
    public float StunDuration = 1.5f;

    [Header("Attack Timings (Duración del Estado)")]
    public float AttackWindUpTime = 0.5f;  // Estos controlan la duración del *estado* de ataque,
    public float AttackActiveTime = 0.3f;  // no directamente el hitbox si usas Animation Events.
    public float AttackCooldownTime = 1f;

    EnemyBaseState _currentState;
    EnemyStateFactory _states;

    public bool IsAttacking { get; set; } = false; // Para que EnemyAttackTrigger sepa si el ataque está activo
    public bool IsFacingRight { get; private set; } = true;
    private bool _playerHitThisSwing; // Para evitar golpear al jugador múltiples veces en un solo swing

    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        _currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) PlayerReference = playerObj;
        else Debug.LogError("ENEMY: Player no encontrado! Asegúrate que el jugador tenga el tag 'Player'.");

        if (AttackHitbox == null)
        {
            Debug.LogError("ENEMY: El campo 'AttackHitbox' no ha sido asignado en el Inspector de EnemyController!");
        }
        else
        {
            EnemyAttackTrigger triggerScript = AttackHitbox.GetComponent<EnemyAttackTrigger>();
            if (triggerScript != null)
            {
                triggerScript.EnemyOwner = this; // Darle al script del hitbox una referencia a este controlador
            }
            else
            {
                Debug.LogError("ENEMY: El GameObject asignado como AttackHitbox no tiene el script EnemyAttackTrigger!");
            }
            AttackHitbox.SetActive(false); // Asegurarse de que esté desactivado al inicio
        }

        _states = new EnemyStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();
    }

    void Update()
    {
        _currentState.UpdateState();
        _currentState.CheckSwitchStates();
    }

    void FixedUpdate()
    {
        _currentState.FixedUpdateState();
    }

    public void ChangeState(EnemyBaseState newState)
    {
        _currentState?.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }

    // --- Métodos para ser llamados por Animation Events del Ataque del Enemigo ---
    public void EnableEnemyAttackHitbox()
    {
        if (AttackHitbox != null)
        {
            _playerHitThisSwing = false; // Resetear para el nuevo swing
            AttackHitbox.SetActive(true);
            // Debug.Log("ENEMY: AttackHitbox HABILITADO por Animation Event");
        }
    }

    public void DisableEnemyAttackHitbox()
    {
        if (AttackHitbox != null)
        {
            AttackHitbox.SetActive(false);
            // Debug.Log("ENEMY: AttackHitbox DESHABILITADO por Animation Event");
        }
    }

    // --- Método para que EnemyAttackTrigger.cs compruebe si puede golpear ---
    public bool CanHitPlayerThisSwing()
    {
        if (_playerHitThisSwing)
        {
            return false; // Ya golpeó al jugador en este swing
        }
        else
        {
            _playerHitThisSwing = true; // Marcar como golpeado
            return true;
        }
    }

    public void AttackParried()
    {
        Debug.Log("ENEMY: ¡Mi ataque fue parreado!");
        IsAttacking = false; // Detener la lógica de ataque actual si es parreado
        if (AttackHitbox != null && AttackHitbox.activeSelf)
        {
            AttackHitbox.SetActive(false); // Desactivar el hitbox inmediatamente
        }
        ChangeState(_states.Stunned());
    }

    public void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void TakeDamage(int damageAmount)
    {
        if (_currentHealth <= 0 && gameObject.activeInHierarchy) return;

        _currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} recibió {damageAmount} de daño. Vida restante: {_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Anim?.SetTrigger("Hurt");
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        _currentHealth = 0;
        Anim?.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        if(Rb != null) Rb.simulated = false;
        this.enabled = false; // Desactiva este script
        // Destroy(gameObject, 2f); // Opcional: destruir después de un tiempo
    }

    // Métodos de ejemplo para controlar animaciones (si tus estados no lo hacen directamente)
    public void SetAnimationBool(string boolName, bool value)
    {
        Anim?.SetBool(boolName, value);
    }
    public void TriggerAnimation(string triggerName)
    {
        Anim?.SetTrigger(triggerName);
    }
}