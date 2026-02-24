using UnityEngine;
using TMPro;

[System.Serializable]
public class XrayLevel
{
    public string patientName;    
    public Texture referenceTex;  
    public Texture questionTex;   
    public bool isActuallySick;   
}

public class XrayGameManager : MonoBehaviour
{
    [Header("3D Экраны (Quads)")]
    public Renderer referenceRenderer; 
    public Renderer questionRenderer;

    [Header("Текстовое табло (TextMeshPro)")]
    public TextMeshPro feedbackText; 

    [Header("База данных уровней")]
    public XrayLevel[] levels; 
    
    private int currentIndex = 0;
    private bool canAnswer = true;

    // Переменная для подсчета правильных ответов
    private int correctAnswersCount = 0;

    void Start()
    {
        if (referenceRenderer == null || questionRenderer == null)
        {
            Debug.LogError("Назначьте Renderer (Quad) в инспекторе!");
            return;
        }

        if (levels == null || levels.Length == 0)
        {
            if (feedbackText != null) feedbackText.text = "Добавьте уровни в инспекторе!";
            return;
        }

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (currentIndex < levels.Length)
        {
            if (levels[currentIndex].referenceTex != null)
                referenceRenderer.material.mainTexture = levels[currentIndex].referenceTex;
            
            if (levels[currentIndex].questionTex != null)
                questionRenderer.material.mainTexture = levels[currentIndex].questionTex;

            if (feedbackText != null) 
                feedbackText.text = "Пациент: " + levels[currentIndex].patientName;
            
            canAnswer = true;
        }
        else
        {
            // КОНЕЦ ИГРЫ: Показываем финальный результат
            ShowFinalResult();
        }
    }

    public void ProcessAnswer(bool playerSaysSick)
    {
        if (!canAnswer || currentIndex >= levels.Length) return;
        
        canAnswer = false; 
        bool correctAnswer = levels[currentIndex].isActuallySick;

        if (playerSaysSick == correctAnswer)
        {
            correctAnswersCount++; // Увеличиваем счетчик при правильном ответе
            if (feedbackText != null) feedbackText.text = "<color=green>ВЕРНО!</color>";
        }
        else
        {
            if (feedbackText != null) feedbackText.text = "<color=red>ОШИБКА!</color>";
        }

        Invoke("NextLevel", 1.5f);
    }

    void NextLevel()
    {
        currentIndex++;
        UpdateDisplay();
    }

    // Метод для вывода финальной статистики
    void ShowFinalResult()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "<color=yellow>СМЕНА ОКОНЧЕНА</color>\n" + 
                                "Правильно: " + correctAnswersCount + " из " + levels.Length;
        }
        
        // Очищаем экраны, чтобы они не светились картинками в конце
        referenceRenderer.material.mainTexture = null;
        questionRenderer.material.mainTexture = null;
    }
}