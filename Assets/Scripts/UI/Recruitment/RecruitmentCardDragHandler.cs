using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RecruitmentEmployeeCardUI))]
public class RecruitmentCardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Layer")]
    [SerializeField] private Transform dragLayer;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private RecruitmentEmployeeCardUI cardUI;

    private Transform originalParent;
    private int originalSiblingIndex;
    private bool droppedOnValidTarget;

    public RecruitmentEmployeeCardUI CardUI => cardUI;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardUI = GetComponent<RecruitmentEmployeeCardUI>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (dragLayer == null)
        {
            GameObject dragLayerObject = GameObject.Find("DragLayer");

            if (dragLayerObject != null)
                dragLayer = dragLayerObject.transform;
        }

        if (dragLayer == null && rootCanvas != null)
            dragLayer = rootCanvas.transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardUI == null || cardUI.Employee == null)
            return;

        if (dragLayer == null)
            return;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        droppedOnValidTarget = false;

        transform.SetParent(dragLayer, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvas == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!droppedOnValidTarget)
        {
            cardUI.MoveToParent(
                originalParent,
                originalSiblingIndex
            );

            return;
        }

        if (cardUI.CurrentRosterSlot != null)
            cardUI.CurrentRosterSlot.SetCard(cardUI);
    }

    public void MarkDroppedOnValidTarget()
    {
        droppedOnValidTarget = true;
    }
}