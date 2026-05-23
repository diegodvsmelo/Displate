using UnityEngine;
using UnityEngine.EventSystems; 

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Transform originalParent; 

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {   
        originalParent = transform.parent;
        
        transform.SetParent(transform.root); 
        
        canvasGroup.alpha = 0.6f;
        
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / transform.lossyScale.x; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {        
        canvasGroup.alpha = 1f;
        
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }
}