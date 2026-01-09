using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TaskSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pinPrefab; // O prefab que criamos acima
    public RectTransform spawnArea; // O painel onde eles vão aparecer
    
    [Header("Timers")]
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;

    [Header("Data Source")]
    public List<TaskData> possibleTasks; // Lista de missões possíveis

    private bool isSpawningActive = false;
    private GameManager gameManager; // Referência para abrir a janela

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Começa a spawnar assim que o jogo abre (ou você pode chamar isso depois)
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (!isSpawningActive)
        {
            isSpawningActive = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawningActive)
        {
            // 1. Espera um tempo aleatório
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // 2. Cria a Task
            SpawnTask();
        }
    }

    void SpawnTask()
    {
        if (possibleTasks.Count == 0) return;

        // Escolhe uma missão aleatória da lista
        TaskData randomTask = possibleTasks[Random.Range(0, possibleTasks.Count)];

        // Cria o objeto visual
        GameObject newPin = Instantiate(pinPrefab, spawnArea);

        // --- LÓGICA DE POSIÇÃO ALEATÓRIA ---
        // Pega o tamanho do painel
        float width = spawnArea.rect.width;
        float height = spawnArea.rect.height;

        // Calcula X e Y aleatórios (assumindo que o pivot do painel é o centro 0.5, 0.5)
        // Se o pivot for (0,0), remova a divisão por 2.
        float randomX = Random.Range(-width / 2f, width / 2f);
        float randomY = Random.Range(-height / 2f, height / 2f);

        // Aplica a posição com uma margem de segurança (padding) de 50px para não ficar na borda
        newPin.GetComponent<RectTransform>().anchoredPosition = new Vector2(randomX * 0.9f, randomY * 0.9f);

        // Configura o pino e diz: "Quando clicar, chame a função OnTaskPinClicked"
        TaskPin pinScript = newPin.GetComponent<TaskPin>();
        pinScript.Setup(randomTask, OnTaskPinClicked);
    }

    // Essa função roda quando o jogador clica num pino
    void OnTaskPinClicked(TaskData task)
    {
        Debug.Log($"Jogador clicou na missão: {task.taskName}");
        
        // 1. Para de spawnar coisas
        StopSpawning();

        // 2. Avisa o GameManager para abrir a janela de missão
        gameManager.OpenMissionWindow(task);
    }
}