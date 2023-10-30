using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float blinkDuration = 0.5f; // Time for each alpha transition

    private Coroutine blinkCoroutine;

    private void Start()
    {
        StartBlink();
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            yield return StartCoroutine(FadeAlpha(0, 1, blinkDuration)); // Fade out
            yield return StartCoroutine(FadeAlpha(1, 0, blinkDuration)); // Fade in
        }
    }

    private IEnumerator FadeAlpha(float startAlpha, float targetAlpha, float duration)
    {
        if (targetAlpha == 0)
        {
            yield return new WaitForSeconds(.5f);
        }
        Color startColor = _text.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float elapsedTime = Time.time - startTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            _text.color = Color.Lerp(startColor, targetColor, normalizedTime);
            yield return null;
        }

        _text.color = targetColor; // Ensure that the color is exactly the target color at the end
    }

    private void StartBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(Blink());
    }

    private void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
    }
}
