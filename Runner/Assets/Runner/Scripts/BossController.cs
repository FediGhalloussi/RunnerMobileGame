using System.Collections;
using UnityEngine;
using Microlight.MicroBar;

public class BossController : MonoBehaviour
{
    [SerializeField] private float dirtyThreshold = 100f;
    [SerializeField] private float cleanSpeed = 0.1f;
    [SerializeField] MicroBar _hpBar;

    private Renderer bossRenderer;
    private Material bossMaterial;

    private void Start()
    {
        bossRenderer = GetComponent<Renderer>();
        bossMaterial = bossRenderer.material;
        // HealthBar needs to be initalized at start
        if (_hpBar != null) _hpBar.Initialize(dirtyThreshold);
    }

    private Coroutine cleaningCoroutine;

    public bool CleanBoss(float playerWaterLevel)
    {
        // Stop any ongoing cleaning coroutine
        if (cleaningCoroutine != null)
        {
            StopCoroutine(cleaningCoroutine);
        }

        // Get the boss's current dirtiness level
        float initialDirtiness = bossMaterial.GetFloat("_DirtEffect");

        // Calculate the target dirtiness level based on player's water level
        float targetDirtiness = Mathf.Max(0, initialDirtiness - (playerWaterLevel / dirtyThreshold));

        // Start the cleaning coroutine
        cleaningCoroutine = StartCoroutine(SmoothCleanBoss(initialDirtiness, targetDirtiness));

        // Update HealthBar
        if (_hpBar != null) _hpBar.UpdateHealthBar(targetDirtiness* dirtyThreshold);

        return targetDirtiness <= 0;
    }

    private IEnumerator SmoothCleanBoss(float startDirtiness, float targetDirtiness)
    {
        float startTime = Time.time;
        float elapsedTime = 0;

        while (elapsedTime < cleanSpeed)
        {
            float normalizedTime = elapsedTime / cleanSpeed;
            float currentDirtiness = Mathf.Lerp(startDirtiness, targetDirtiness, normalizedTime);

            bossMaterial.SetFloat("_DirtEffect", currentDirtiness);

            elapsedTime = Time.time - startTime;
            yield return null;
        }

        // Ensure that the final dirtiness value is set
        bossMaterial.SetFloat("_DirtEffect", targetDirtiness);

        cleaningCoroutine = null; // Reset the coroutine reference
    }

}
