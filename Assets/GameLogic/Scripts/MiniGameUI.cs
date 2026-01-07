using UnityEngine;
using UnityEngine.UI;

public class MinigameUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform successZone; // A barra verde
    public RectTransform pointer;     // A agulha branca
    
    [Header("Settings")]
    public float totalWidth = 400f;   // Largura total do fundo
    public float pointerSpeed = 300f; // Velocidade da agulha

    // Controle interno (Não mexa pelo Inspector)
    private bool isRunning = true; 
    private float finalPositionX = 0f;

    void Update()
    {
        // AQUI ESTAVA O PROBLEMA:
        // Removemos o 'SetZoneSize(testChance)' daqui.
        // Agora o tamanho só muda quando o GameManager manda.

        if (isRunning)
        {
            // Enquanto o jogo roda, a agulha vai e volta
            MovePointer(); 
        }
        else
        {
            // Quando o jogo acaba, a agulha vai suavemente para o resultado
            MovePointerToResult(); 
        }
    }

    // Chamado pelo GameManager no início
    public void SetZoneSize(float chancePercent)
    {
        float newWidth = (totalWidth * chancePercent) / 100f;
        successZone.sizeDelta = new Vector2(newWidth, successZone.sizeDelta.y);
    }

    // Chamado pelo GameManager no final (após 3 segundos)
    public void StopPointer(float targetX)
    {
        isRunning = false; // Isso trava o 'MovePointer' no Update
        finalPositionX = targetX;
    }

    void MovePointer()
    {
        float xPosition = Mathf.PingPong(Time.time * pointerSpeed, totalWidth);
        pointer.anchoredPosition = new Vector2(xPosition, 0);
    }

    void MovePointerToResult()
    {
        float currentX = pointer.anchoredPosition.x;
        // Move suavemente até a posição sorteada
        float smoothX = Mathf.Lerp(currentX, finalPositionX, Time.deltaTime * 5f);
        pointer.anchoredPosition = new Vector2(smoothX, 0);
    }
}