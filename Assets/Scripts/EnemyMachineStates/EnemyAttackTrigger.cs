using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    public EnemyController EnemyOwner { get; set; } // Se asigna desde EnemyController en Awake

    void OnTriggerEnter2D(Collider2D other)
    {
        // No hacer nada si no hay dueño, el dueño no está en su lógica de ataque, o el hitbox no está realmente activo
        if (EnemyOwner == null || !EnemyOwner.IsAttacking || !gameObject.activeInHierarchy)
        {
            return;
        }

        // 1. Detección de Parry del Jugador
        if (other.gameObject.CompareTag("PlayerParryBox")) // Tag del ParryBox del Jugador
        {
            Debug.Log("ENEMY HITBOX: Ataque del Enemigo PARREADO por el Jugador!");
            EnemyOwner.AttackParried();
            // Desactivar este hitbox para que no pueda golpear al jugador si el parry y el golpe ocurren casi al mismo tiempo.
            // La animación lo desactivará eventualmente, pero esto es una seguridad adicional.
            gameObject.SetActive(false); 
            return; // Si fue parreado, no procesar daño al jugador.
        }
        // 2. Detección de Golpe al Jugador (si no fue parreado)
        else if (other.gameObject.CompareTag("Player")) // Tag del GameObject principal del Jugador
        {
            if (EnemyOwner.CanHitPlayerThisSwing()) // Comprobar si ya golpeó al jugador en este swing
            {
                PlayerStateMachine player = other.GetComponent<PlayerStateMachine>();
                if (player != null && !player.IsDead) // Asegurarse que el jugador no esté muerto
                {
                    Debug.Log($"ENEMY HITBOX: Enemigo golpeó a {player.name} por {EnemyOwner.attackDamage} de daño.");
                    player.TakeDamage(EnemyOwner.attackDamage);
                }
                else if (player == null)
                {
                    Debug.LogWarning($"ENEMY HITBOX: El objeto {other.name} con tag 'Player' no tiene PlayerStateMachine.");
                }
            }
        }
    }
}