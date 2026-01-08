using UnityEngine;
using UnityEngine.EventSystems; // Necessário para os eventos

public class Slot : MonoBehaviour, IDropHandler
{
    // Esta função roda AUTOMATICAMENTE quando você solta algo com "Draggable" aqui em cima
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Algo caiu no slot!");

        // 1. Verifica se o objeto que caiu tem o script Draggable
        if (eventData.pointerDrag != null)
        {
            // Pega o script da carta que está sendo arrastada
            Draggable draggableScript = eventData.pointerDrag.GetComponent<Draggable>();
            
            if (draggableScript != null)
            {
                // 2. O PULO DO GATO:
                // Mudamos o 'originalParent' da carta para SER ESTE SLOT.
                draggableScript.originalParent = this.transform;
                
                // Agora, quando o 'OnEndDrag' da carta rodar (milissegundos depois),
                // ele vai mover a carta para o centro deste slot, e não para o menu anterior.
            }
        }
    }
}