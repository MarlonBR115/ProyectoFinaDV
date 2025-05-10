// PlayerParryHitbox.cs
using UnityEngine;

public class PlayerParryHitbox : MonoBehaviour
{
    private PlayerStateMachine _playerStateMachine;

    void Awake()
    {
        // Asumimos que PlayerStateMachine está en el padre del ParryBox
        _playerStateMachine = GetComponentInParent<PlayerStateMachine>();
        if (_playerStateMachine == null)
        {
            Debug.LogError("PlayerStateMachine no encontrado en el padre de PlayerParryHitbox!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Asegúrate de que el PlayerParryState sea el que controle si este hitbox debe estar activo
        // (ya que el estado PlayerParryState activa/desactiva este GameObject)

        // Comprobar si colisionó con un ataque de enemigo (ej. tag "EnemyAttack")
        if (other.gameObject.CompareTag("EnemyAttack"))
        {
            Debug.Log("Parry SUCCESSFUL!");
            if (_playerStateMachine != null)
            {
                _playerStateMachine.NotifyParrySuccess();
            }
            // Desactivar este colisionador para evitar múltiples detecciones de parry en un solo uso
            // El GameObject ParryBox completo será desactivado por PlayerParryState al salir.
            // Pero si el parry es exitoso, podríamos querer desactivar el collider inmediatamente.
            GetComponent<Collider2D>().enabled = false;
        }
    }

    // El PlayerParryState se encargará de reactivar el collider cuando el ParryBox se reactive.
    // Podemos añadir un método para esto:
    public void ResetCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }
}