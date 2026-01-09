using UnityEngine;
using UnityEngine.EventSystems; 

// Essas interfaces obrigam a gente a criar as funções OnBeginDrag, OnDrag e OnEndDrag
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
        
        // Tira a carta da lista e joga direto no Canvas para ela ficar por cima de tudo
        // (Assumindo que o Canvas é o avô ou bisavô)
        transform.SetParent(transform.root); 
        
        // Deixa a carta meio transparente
        canvasGroup.alpha = 0.6f;
        
        // O PULO DO GATO: Desliga o Raycast. 
        // Agora o mouse "atravessa" a carta e consegue ver o que está embaixo (o Slot)
        canvasGroup.blocksRaycasts = false;
    }

    // 2. Enquanto você move o mouse (Isso roda a cada frame)
    public void OnDrag(PointerEventData eventData)
    {
        // A carta segue a posição do mouse
        rectTransform.anchoredPosition += eventData.delta / transform.lossyScale.x; 
        // (A divisão pelo scale corrige a velocidade se o Canvas tiver zoom)
    }

    // 3. Quando você solta o botão do mouse
    public void OnEndDrag(PointerEventData eventData)
    {        
        // Volta a ficar opaca
        canvasGroup.alpha = 1f;
        
        // Liga o Raycast de novo para poder clicar nela futuramente
        canvasGroup.blocksRaycasts = true;

        // Por enquanto, sempre volta para casa (snap back)
        // Depois vamos mudar isso para "Se achou um slot, fica lá"
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero; // Reseta a posição local
    }
}