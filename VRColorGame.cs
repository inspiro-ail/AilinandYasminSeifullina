using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;
using TMPro;

public class VRColorGame : MonoBehaviour
{
    [Header("Настройки объектов")]
    public GameObject cellPrefab;
    
    [Header("3D Текст (TextMeshPro)")]
    public TextMeshPro statsText; 

    [Header("Параметры сетки")]
    public float spacing = 0.5f;
    public float cellScale = 0.4f;

    [Header("Сложность")]
    public float colorDiff = 0.12f;
    public int roundsToWin = 10; 

    private List<GameObject> _cells = new List<GameObject>();
    private int _targetIndex;
    private int _currentRound = 0; 
    private int _correctAnswers = 0;
    private int _wrongAnswers = 0;
    private bool _isGameOver = false;

    void Start()
    {
        if (statsText != null) 
        {
            statsText.color = Color.white;
            statsText.text = "Найди лишний цвет!";
        }
        
        SpawnGrid();
        Invoke("UpdateUI", 0.1f);
        NextRound();
    }

    void SpawnGrid()
    {
        foreach (var c in _cells) Destroy(c);
        _cells.Clear();

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                cell.transform.localPosition = new Vector3((x - 1) * spacing, (y - 1) * spacing, 0);
                cell.transform.localScale = Vector3.one * cellScale;

                XRSimpleInteractable interactable = cell.GetComponent<XRSimpleInteractable>();
                if (interactable != null)
                {
                    int index = _cells.Count; 
                    interactable.selectEntered.AddListener(e => OnCellClick(index));
                }
                _cells.Add(cell);
            }
        }
    }

    void NextRound()
    {
        if (_currentRound >= roundsToWin)
        {
            ShowVictory();
            return;
        }

        Color mainColor = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.5f, 1f);
        _targetIndex = Random.Range(0, _cells.Count);
        
        float offset = (Random.value > 0.5f) ? colorDiff : -colorDiff;
        Color hiddenColor = new Color(
            Mathf.Clamp01(mainColor.r + offset),
            Mathf.Clamp01(mainColor.g + offset),
            Mathf.Clamp01(mainColor.b + offset)
        );

        for (int i = 0; i < _cells.Count; i++)
        {
            var rend = _cells[i].GetComponent<Renderer>();
            if (rend != null) rend.material.color = (i == _targetIndex) ? hiddenColor : mainColor;
        }
        
        colorDiff *= 0.92f; 
    }

    void OnCellClick(int index)
    {
        if (_isGameOver) return; 

        _currentRound++; 

        if (index == _targetIndex)
        {
            _correctAnswers++;
        }
        else
        {
            _wrongAnswers++;
            StartCoroutine(FlashRedEffect());
        }

        UpdateUI();
        NextRound(); 
    }

    void UpdateUI()
    {
        if (statsText != null && !_isGameOver)
        {
            statsText.text = $"Раунд: {_currentRound}/{roundsToWin}\n" +
                             $"<color=green>Верно: {_correctAnswers}</color> | " +
                             $"<color=red>Ошибки: {_wrongAnswers}</color>";
        }
    }

    System.Collections.IEnumerator FlashRedEffect()
    {
        if (statsText != null)
        {
            statsText.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            if (!_isGameOver) statsText.color = Color.white;
        }
    }

    void ShowVictory()
    {
        _isGameOver = true;

        
        foreach (GameObject cell in _cells) cell.SetActive(false);
        
        if (statsText != null)
        {
            statsText.color = Color.yellow;
            
            
            string rating = "";
            if (_wrongAnswers == 0) rating = "СУПЕР! ТЫ МАСТЕР!";
            else if (_wrongAnswers < 3) rating = "ОТЛИЧНО!";
            else rating = "ХОРОШО, НО МОЖНО ЛУЧШЕ!";

            statsText.text = $"{rating}\n" +
                             $"<color=white>Счет: {_correctAnswers} из {roundsToWin}</color>\n" +
                             $"<color=red>Ошибок: {_wrongAnswers}</color>";
        }
    }

}
