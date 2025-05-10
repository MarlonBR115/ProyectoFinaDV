using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    public int attackDamage = 10;

    private PlayerStateMachine _playerStateMachine;

    void Awake()
    {
        _playerStateMachine = GetComponentInParent<PlayerStateMachine>();
        if (_playerStateMachine == null)
        {
            Debug.LogError("PlayerAttackHitbox: No se pudo encontrar PlayerStateMachine en el padre.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_playerStateMachine == null || !gameObject.activeInHierarchy)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            if (_playerStateMachine.CanHitTarget(other))
            {
                Debug.Log($"HITBOX: Golpeó a {other.name} por {attackDamage} de daño.");

                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage); // La llamada problemática
                }
                else
                {
                    Debug.LogWarning($"HITBOX: El objeto {other.name} con tag 'Enemy' no tiene un script EnemyController adjunto o el script tiene errores de compilación.");
                }
            }
        }
    }
}