using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReputationUI : MonoBehaviour
{
    [Header("Stars")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite emptyStarSprite;
    [SerializeField] private Sprite filledStarSprite;

    [Header("Progress")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private int reputationPerStar = 100;
    [SerializeField] private int maxStars = 5;

    [Header("Animation")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float stepDelay = 0.02f;

    private int displayedReputation;
    private Coroutine animationRoutine;
    private ResourceManager resourceManager;
    private ReputationTierManager reputationTierManager;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (resourceManager != null)
            resourceManager.OnReputationChanged -= HandleReputationChanged;

        if (reputationTierManager != null)
        {
            reputationTierManager.OnCurrentTierChanged -= HandleCurrentTierChanged;
            reputationTierManager.OnReputationCapChanged -= HandleReputationCapChanged;
        }

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        resourceManager = null;
        reputationTierManager = null;
    }

    private void TrySubscribe()
    {
        if (resourceManager == null && ResourceManager.Instance != null)
        {
            resourceManager = ResourceManager.Instance;
            resourceManager.OnReputationChanged += HandleReputationChanged;

            displayedReputation = resourceManager.CurrentReputation;
            UpdateReputationUI(displayedReputation);
        }

        if (reputationTierManager == null && ReputationTierManager.Instance != null)
        {
            reputationTierManager = ReputationTierManager.Instance;
            reputationTierManager.OnCurrentTierChanged += HandleCurrentTierChanged;
            reputationTierManager.OnReputationCapChanged += HandleReputationCapChanged;

            UpdateReputationUI(displayedReputation);
        }
    }

    private void HandleReputationChanged(int newReputation)
    {
        newReputation = Mathf.Clamp(newReputation, 0, GetMaxReputation());

        if (!animateChanges)
        {
            displayedReputation = newReputation;
            UpdateReputationUI(displayedReputation);
            return;
        }

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimateReputationChange(newReputation));
    }

    private void HandleCurrentTierChanged(ReputationTierData currentTier)
    {
        if (resourceManager != null)
            displayedReputation = resourceManager.CurrentReputation;

        UpdateReputationUI(displayedReputation);
    }

    private void HandleReputationCapChanged(int newCap)
    {
        if (resourceManager != null)
            displayedReputation = resourceManager.CurrentReputation;

        UpdateReputationUI(displayedReputation);
    }

    private IEnumerator AnimateReputationChange(int targetReputation)
    {
        targetReputation = Mathf.Clamp(targetReputation, 0, GetMaxReputation());

        while (displayedReputation != targetReputation)
        {
            if (displayedReputation < targetReputation)
                displayedReputation++;
            else
                displayedReputation--;

            UpdateReputationUI(displayedReputation);

            yield return new WaitForSeconds(stepDelay);
        }

        animationRoutine = null;
    }

    private void UpdateReputationUI(int totalReputation)
    {
        int minReputation = GetCurrentTierMinimumReputation();
        int maxReputation = Mathf.Max(minReputation + 1, GetMaxReputation());
        int tierRange = Mathf.Max(1, maxReputation - minReputation);
        int tierProgress = Mathf.Clamp(totalReputation - minReputation, 0, tierRange);

        int visibleStars = GetVisibleStarsCount();
        float reputationPerVisibleStar = tierRange / (float)visibleStars;

        int filledStars = tierProgress >= tierRange
            ? visibleStars
            : Mathf.FloorToInt(tierProgress / reputationPerVisibleStar);

        float remainder = tierProgress - (filledStars * reputationPerVisibleStar);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
                continue;

            if (i >= visibleStars)
            {
                starImages[i].sprite = emptyStarSprite;
                continue;
            }

            starImages[i].sprite = i < filledStars ? filledStarSprite : emptyStarSprite;
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = reputationPerVisibleStar;

            bool reachedCap = tierProgress >= tierRange;
            bool exactStarValue = Mathf.Approximately(remainder, 0f);

            if (tierProgress <= 0 || reachedCap || exactStarValue)
                progressSlider.value = 0f;
            else
                progressSlider.value = remainder;
        }

        UpdateFillVisibility(tierProgress, tierRange, remainder);
    }

    private void UpdateFillVisibility(int tierProgress, int tierRange, float remainder)
    {
        if (fillImage == null)
            return;

        bool reachedCap = tierProgress >= tierRange;
        bool exactStarValue = Mathf.Approximately(remainder, 0f);

        bool shouldHide =
            tierProgress <= 0 ||
            reachedCap ||
            exactStarValue;

        Color color = fillImage.color;
        color.a = shouldHide ? 0f : 1f;
        fillImage.color = color;
    }

    private int GetCurrentTierMinimumReputation()
    {
        if (ReputationTierManager.Instance != null)
            return ReputationTierManager.Instance.CurrentTierMinimumReputation;

        return 0;
    }

    private int GetMaxReputation()
    {
        if (ReputationTierManager.Instance != null)
            return ReputationTierManager.Instance.CurrentReputationCap;

        return reputationPerStar * maxStars;
    }

    private int GetVisibleStarsCount()
    {
        int starCount = starImages != null ? starImages.Length : 0;
        return Mathf.Max(1, Mathf.Min(maxStars, starCount));
    }
}
