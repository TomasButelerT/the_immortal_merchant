using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;

    private float shakeTimeRemaining;
    private float shakeDuration;
    private float shakeStrength;

    public void Shake(float duration, float strength)
    {
        shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, duration);
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeStrength = Mathf.Max(shakeStrength, strength);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.unscaledDeltaTime);

        if (shakeTimeRemaining > 0f)
        {
            float intensity = shakeDuration > 0f ? shakeTimeRemaining / shakeDuration : 0f;
            Vector2 offset = Random.insideUnitCircle * shakeStrength * intensity;
            transform.position += new Vector3(offset.x, offset.y, 0f);
            shakeTimeRemaining -= Time.unscaledDeltaTime;
        }
        else
        {
            shakeDuration = 0f;
            shakeStrength = 0f;
        }
    }
}
