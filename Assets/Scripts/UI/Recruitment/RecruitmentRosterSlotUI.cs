using UnityEngine;
using UnityEngine.EventSystems;

public class RecruitmentRosterSlotUI : MonoBehaviour, IDropHandler
{
    private StaffRecruitmentManager owner;

    public RecruitmentEmployeeCardUI CurrentCard { get; private set; }
    public bool IsEmpty => CurrentCard == null;

    public void Setup(StaffRecruitmentManager ownerManager)
    {
        owner = ownerManager;
    }

    public void OnDrop(PointerEventData eventData)
    {
        RecruitmentCardDragHandler dragHandler = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<RecruitmentCardDragHandler>()
            : null;

        if (dragHandler == null || dragHandler.CardUI == null)
            return;

        bool accepted = owner != null && owner.TryDropCardOnRosterSlot(dragHandler.CardUI, this);

        if (accepted)
            dragHandler.MarkDroppedOnValidTarget();
    }

    public void SetCard(RecruitmentEmployeeCardUI card)
    {
        if (card == null)
            return;

        if (CurrentCard != null && CurrentCard != card)
            CurrentCard.SetCurrentRosterSlot(null);

        CurrentCard = card;
        CurrentCard.SetCurrentRosterSlot(this);

        Transform cardTransform = CurrentCard.transform;
        cardTransform.SetParent(transform, false);
        cardTransform.SetAsLastSibling();

        ForceCardCentered(CurrentCard);
    }

    public void ClearCard(RecruitmentEmployeeCardUI card)
    {
        if (CurrentCard != card)
            return;

        CurrentCard = null;

        if (card != null)
            card.SetCurrentRosterSlot(null);
    }

    private void ForceCardCentered(RecruitmentEmployeeCardUI card)
    {
        if (card == null)
            return;

        RectTransform rectTransform = card.transform as RectTransform;

        if (rectTransform == null)
            return;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }
}