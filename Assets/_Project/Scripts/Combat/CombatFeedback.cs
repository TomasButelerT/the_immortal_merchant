using System.Collections;
using UnityEngine;

public class CombatFeedback : MonoBehaviour
{
    private static CombatFeedback instance;

    private Coroutine hitStopRoutine;
    private float normalFixedDeltaTime;

    public static void PlayHit(float hitStopDuration, float shakeDuration, float shakeStrength)
    {
        EnsureInstance();
        instance.StartHitStop(hitStopDuration);

        CameraFollow2D cameraFollow = FindAnyObjectByType<CameraFollow2D>();
        if (cameraFollow != null)
        {
            cameraFollow.Shake(shakeDuration, shakeStrength);
        }
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject feedbackObject = new GameObject("CombatFeedback");
        instance = feedbackObject.AddComponent<CombatFeedback>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        normalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void StartHitStop(float duration)
    {
        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
        }

        hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = normalFixedDeltaTime * Time.timeScale;
        yield return new WaitForSecondsRealtime(duration);
        RestoreTimeScale();
        hitStopRoutine = null;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            RestoreTimeScale();
            instance = null;
        }
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
    }
}
