using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public bool isRoster = false;
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            Draggable incomingDraggable = eventData.pointerDrag.GetComponent<Draggable>();
            
            if (incomingDraggable != null)
            {
                if (transform.childCount > 0)
                {
                    Transform existingCard = transform.GetChild(0);

                    Transform previousHome = incomingDraggable.originalParent;

                    existingCard.SetParent(previousHome);
                    
                    existingCard.localPosition = Vector3.zero;
                }

                incomingDraggable.originalParent = this.transform;
            }
        }
    }
}