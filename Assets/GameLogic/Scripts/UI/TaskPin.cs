using UnityEngine;
using UnityEngine.UI; // Necessário para Button

public class TaskPin : MonoBehaviour
{
    public TaskData data; // A missão que esse pino carrega
    private System.Action<TaskData> onClickCallback; // Quem avisar quando clicar

    // Função chamada pelo Spawner assim que o pino nasce
    public void Setup(TaskData taskData, System.Action<TaskData> callback)
    {
        this.data = taskData;
        this.onClickCallback = callback;
        
        // (Opcional) Aqui você poderia mudar o ícone do botão baseado na taskData.category
    }

    // Função ligada ao botão da UI
    public void OnClick()
    {
        if (onClickCallback != null)
        {
            onClickCallback.Invoke(data);
        }
        
        // Opcional: Destruir o pino ao clicar (ou manter ele lá se a missão persistir)
        Destroy(gameObject); 
    }
}