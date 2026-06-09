using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitmentPayrollIndicatorUI : MonoBehaviour
{
    private enum PayrollTrend
    {
        Same,
        Cheaper,
        MoreExpensive
    }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI payrollText;
    [SerializeField] private Image arrowImage;

    [Header("Colors")]
    [SerializeField] private Color cheaperColor = Color.green;
    [SerializeField] private Color moreExpensiveColor = Color.red;
    [SerializeField] private Color sameValueColor = Color.gray;

    [Header("Arrow Rotation")]
    [Tooltip("Rotacao usada quando o custo ficou menor. Considera que a sprite padrao aponta para cima.")]
    [SerializeField] private float cheaperRotationZ = 0f;

    [Tooltip("Rotacao usada quando o custo ficou maior.")]
    [SerializeField] private float moreExpensiveRotationZ = 180f;

    [Tooltip("Rotacao usada quando o custo ficou igual.")]
    [SerializeField] private float sameValueRotationZ = -90f;

    [Header("Transition Animation")]
    [SerializeField, Min(0.01f)] private float animationDuration = 0.2f;
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Pulse Animation - Cheaper")]
    [SerializeField] private bool pulseWhenCheaper = true;
    [SerializeField, Min(0.01f)] private float pulseDuration = 0.65f;
    [SerializeField, Min(1f)] private float pulseScale = 1.08f;

    [Header("Shake Animation - More Expensive")]
    [SerializeField] private bool shakeWhenMoreExpensive = true;
    [SerializeField, Min(0.01f)] private float shakeCycleDuration = 0.35f;
    [SerializeField, Min(0f)] private float shakeStrength = 4f;

    [Header("Text Format")]
    [SerializeField] private string prefix = "$";
    [SerializeField] private string suffix = "/DAY";

    private Coroutine transitionCoroutine;
    private Coroutine attentionCoroutine;

    private RectTransform arrowRect;
    private Vector3 baseArrowScale = Vector3.one;
    private Vector2 baseArrowAnchoredPosition = Vector2.zero;

    private int lastCurrentPayroll = -1;
    private int lastOriginalPayroll = -1;
    private PayrollTrend currentTrend = PayrollTrend.Same;

    private void Awake()
    {
        if (arrowImage != null)
        {
            arrowRect = arrowImage.rectTransform;
            baseArrowScale = arrowRect.localScale;
            baseArrowAnchoredPosition = arrowRect.anchoredPosition;
        }
    }

    private void OnDisable()
    {
        StopAllAnimations();
        ResetArrowTransform();
    }

    public void Refresh(int currentPayroll, int originalPayroll)
    {
        currentPayroll = Mathf.Max(0, currentPayroll);
        originalPayroll = Mathf.Max(0, originalPayroll);

        if (payrollText != null)
            payrollText.text = $"{prefix}{currentPayroll}{suffix}";

        PayrollTrend targetTrend = GetTrend(currentPayroll, originalPayroll);
        Color targetColor = GetColorForTrend(targetTrend);
        float targetRotationZ = GetRotationForTrend(targetTrend);

        bool valuesAreSame =
            currentPayroll == lastCurrentPayroll &&
            originalPayroll == lastOriginalPayroll;

        lastCurrentPayroll = currentPayroll;
        lastOriginalPayroll = originalPayroll;

        if (valuesAreSame && targetTrend == currentTrend)
            return;

        currentTrend = targetTrend;

        StartTransition(targetColor, targetRotationZ, targetTrend);
    }

    private PayrollTrend GetTrend(int currentPayroll, int originalPayroll)
    {
        if (currentPayroll < originalPayroll)
            return PayrollTrend.Cheaper;

        if (currentPayroll > originalPayroll)
            return PayrollTrend.MoreExpensive;

        return PayrollTrend.Same;
    }

    private Color GetColorForTrend(PayrollTrend trend)
    {
        switch (trend)
        {
            case PayrollTrend.Cheaper:
                return cheaperColor;

            case PayrollTrend.MoreExpensive:
                return moreExpensiveColor;

            default:
                return sameValueColor;
        }
    }

    private float GetRotationForTrend(PayrollTrend trend)
    {
        switch (trend)
        {
            case PayrollTrend.Cheaper:
                return cheaperRotationZ;

            case PayrollTrend.MoreExpensive:
                return moreExpensiveRotationZ;

            default:
                return sameValueRotationZ;
        }
    }

    private void StartTransition(Color targetColor, float targetRotationZ, PayrollTrend targetTrend)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        if (attentionCoroutine != null)
        {
            StopCoroutine(attentionCoroutine);
            attentionCoroutine = null;
        }

        ResetArrowTransform();

        transitionCoroutine = StartCoroutine(TransitionRoutine(targetColor, targetRotationZ, targetTrend));
    }

    private IEnumerator TransitionRoutine(Color targetColor, float targetRotationZ, PayrollTrend targetTrend)
    {
        float elapsed = 0f;

        Color startTextColor = payrollText != null ? payrollText.color : targetColor;
        Color startArrowColor = arrowImage != null ? arrowImage.color : targetColor;

        float startRotationZ = 0f;

        if (arrowRect != null)
            startRotationZ = arrowRect.localEulerAngles.z;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            float rotationT = rotationCurve != null ? rotationCurve.Evaluate(t) : t;
            float colorT = colorCurve != null ? colorCurve.Evaluate(t) : t;

            float currentRotationZ = Mathf.LerpAngle(startRotationZ, targetRotationZ, rotationT);

            if (arrowImage != null)
            {
                arrowImage.color = Color.Lerp(startArrowColor, targetColor, colorT);
                arrowRect.localRotation = Quaternion.Euler(0f, 0f, currentRotationZ);
            }

            if (payrollText != null)
                payrollText.color = Color.Lerp(startTextColor, targetColor, colorT);

            yield return null;
        }

        ApplyStateInstant(targetColor, targetRotationZ);
        StartAttentionAnimation(targetTrend);

        transitionCoroutine = null;
    }

    private void ApplyStateInstant(Color color, float rotationZ)
    {
        if (payrollText != null)
            payrollText.color = color;

        if (arrowImage != null && arrowRect != null)
        {
            arrowImage.color = color;
            arrowRect.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }
    }

    private void StartAttentionAnimation(PayrollTrend trend)
    {
        if (arrowRect == null)
            return;

        switch (trend)
        {
            case PayrollTrend.Cheaper:
                if (pulseWhenCheaper)
                    attentionCoroutine = StartCoroutine(PulseRoutine());
                break;

            case PayrollTrend.MoreExpensive:
                if (shakeWhenMoreExpensive)
                    attentionCoroutine = StartCoroutine(ShakeRoutine());
                break;

            default:
                ResetArrowTransform();
                break;
        }
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            float elapsed = 0f;

            while (elapsed < pulseDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / pulseDuration);
                float wave = Mathf.Sin(t * Mathf.PI);
                float scaleMultiplier = Mathf.Lerp(1f, pulseScale, wave);

                arrowRect.localScale = baseArrowScale * scaleMultiplier;
                arrowRect.anchoredPosition = baseArrowAnchoredPosition;

                yield return null;
            }

            arrowRect.localScale = baseArrowScale;
            arrowRect.anchoredPosition = baseArrowAnchoredPosition;

            yield return null;
        }
    }

    private IEnumerator ShakeRoutine()
    {
        while (true)
        {
            float elapsed = 0f;

            while (elapsed < shakeCycleDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / shakeCycleDuration);
                float damping = 1f - t;
                float shake = Mathf.Sin(t * Mathf.PI * 6f) * shakeStrength * damping;

                arrowRect.anchoredPosition = baseArrowAnchoredPosition + new Vector2(shake, 0f);
                arrowRect.localScale = baseArrowScale;

                yield return null;
            }

            arrowRect.anchoredPosition = baseArrowAnchoredPosition;
            arrowRect.localScale = baseArrowScale;

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private void StopAllAnimations()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        if (attentionCoroutine != null)
        {
            StopCoroutine(attentionCoroutine);
            attentionCoroutine = null;
        }
    }

    private void ResetArrowTransform()
    {
        if (arrowRect == null)
            return;

        arrowRect.localScale = baseArrowScale;
        arrowRect.anchoredPosition = baseArrowAnchoredPosition;
    }
}