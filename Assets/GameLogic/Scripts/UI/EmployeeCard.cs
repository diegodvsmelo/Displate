using UnityEngine;
using UnityEngine.UI;

public class EmployeeCard : MonoBehaviour
{
    public EmployeeData data; 
    public Image backgroundImage;

    public void Setup(EmployeeData newData)
    {
        this.data = newData;

        // SEGURO: Se a referência estiver vazia, tenta pegar agora
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        // Se conseguiu pegar a imagem, aplica a cor
        if (backgroundImage != null)
        {
            backgroundImage.color = newData.cardColor;
        }
        else
        {
            Debug.LogError("ERRO: Não encontrei o componente Image no Card!");
        }
    }
}