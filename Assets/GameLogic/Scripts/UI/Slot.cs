using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public bool isRoster = false;
    public void OnDrop(PointerEventData eventData)
    {
        // 1. Verifica se o que caiu é uma carta válida
        if (eventData.pointerDrag != null)
        {
            Draggable incomingDraggable = eventData.pointerDrag.GetComponent<Draggable>();
            
            if (incomingDraggable != null)
            {
                // --- A LÓGICA DE TROCA COMEÇA AQUI ---

                // Verifica se este slot JÁ TEM uma carta (filho)
                if (transform.childCount > 0)
                {
                    // Pega a carta que já estava aqui (vamos chamar de "Morador Antigo")
                    Transform existingCard = transform.GetChild(0);

                    // Pega de onde a nova carta veio (vamos chamar de "Casa Anterior")
                    Transform previousHome = incomingDraggable.originalParent;

                    // Manda o Morador Antigo para a Casa Anterior
                    existingCard.SetParent(previousHome);
                    
                    // Reseta a posição dele para ficar centralizado na Casa Anterior
                    existingCard.localPosition = Vector3.zero;

                    // Nota: Se a 'Casa Anterior' for o Container do Roster (com LayoutGroup),
                    // ele vai se ajustar automaticamente na lista.
                }

                // --- FIM DA TROCA ---

                // Define este slot como a nova casa da carta que chegou (Incoming)
                incomingDraggable.originalParent = this.transform;
            }
        }
    }
}