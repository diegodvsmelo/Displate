using UnityEngine;
using UnityEngine.EventSystems;

public class RecruitmentPoolDropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] private StaffRecruitmentManager owner;

    private void Awake()
    {
        if (owner == null)
            owner = FindFirstObjectByType<StaffRecruitmentManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        RecruitmentCardDragHandler dragHandler = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<RecruitmentCardDragHandler>()
            : null;

        if (dragHandler == null || dragHandler.CardUI == null)
            return;

        bool accepted = owner != null && owner.TryDropCardOnPool(dragHandler.CardUI);

        if (accepted)
            dragHandler.MarkDroppedOnValidTarget();
    }
}