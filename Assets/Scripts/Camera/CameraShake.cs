// CameraShake.cs
using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 _originalPosition;
    private Coroutine _currentShakeCoroutine;

    void Start()
    {
        // Guardar la posición original relativa a su padre si lo tiene,
        // o la posición global si no. Para cámaras que siguen a un target (ej. con Cinemachine),
        // este shake básico podría necesitar ajustes o integrarse con el sistema de seguimiento.
        // Para una cámara simple,localPosition es usualmente lo que quieres si es hija de algo.
        // Si es una cámara raíz, position está bien.
        // Por ahora, asumiremos que es la posición local si tiene padre, o global si no.
        if (transform.parent != null)
        {
            _originalPosition = transform.localPosition;
        }
        else
        {
            _originalPosition = transform.position;
        }
    }

    public void StartShake(float duration, float magnitude)
    {
        // Si ya hay un shake en progreso, detenerlo antes de iniciar uno nuevo
        if (_currentShakeCoroutine != null)
        {
            StopCoroutine(_currentShakeCoroutine);
            // Restaurar posición inmediatamente antes del nuevo shake
            if (transform.parent != null) transform.localPosition = _originalPosition;
            else transform.position = _originalPosition;
        }
        _currentShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        Vector3 startPosition; // Posición al inicio de este shake específico

        if (transform.parent != null) startPosition = transform.localPosition;
        else startPosition = transform.position;


        while (elapsed < duration)
        {
            // Generar un desplazamiento aleatorio
            // Usamos Random.insideUnitCircle para un shake 2D más controlado en X e Y.
            // Si quieres shake en Z también, usa Random.insideUnitSphere.
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            // float z = Random.Range(-1f, 1f) * magnitude; // Si quieres shake en Z

            Vector3 shakeOffset = new Vector3(x, y, 0); // O new Vector3(x, y, z);

            if (transform.parent != null)
            {
                transform.localPosition = startPosition + shakeOffset;
            }
            else
            {
                transform.position = startPosition + shakeOffset;
            }

            elapsed += Time.deltaTime;
            yield return null; // Esperar al siguiente frame
        }

        // Restaurar la posición original al finalizar
        if (transform.parent != null)
        {
            transform.localPosition = _originalPosition;
        }
        else
        {
            transform.position = _originalPosition;
        }
        _currentShakeCoroutine = null; // Limpiar la referencia a la corutina
    }

    // Opcional: Si la cámara se mueve por otros medios (ej. seguimiento del jugador),
    // podrías necesitar actualizar _originalPosition periódicamente o al final de cada shake.
    // Por ahora, este setup asume que _originalPosition es la posición de "reposo" de la cámara.
    public void UpdateOriginalPosition()
    {
         if (transform.parent != null)
        {
            _originalPosition = transform.localPosition;
        }
        else
        {
            _originalPosition = transform.position;
        }
    }
}