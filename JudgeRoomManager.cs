using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class JudgeRoomManager : MonoBehaviour
{
    [Header("Настройки Дебатов")]
    public DebateController firstSpeaker; // Перетащи сюда Парламента
    public bool isSessionActive = false;

    [Header("Анимация Кнопки")]
    public float pushDepth = 0.02f; // Глубина нажатия
    public float speed = 10f;      // Скорость анимации
    private Vector3 _startPos;
    private bool _isAnimating = false;

    private XRSimpleInteractable _interactable;

    void Start()
    {
        _startPos = transform.localPosition;
        _interactable = GetComponent<XRSimpleInteractable>();

        // Подписываемся на нажатие триггером (через луч или в упор)
        if (_interactable != null)
        {
            _interactable.selectEntered.AddListener(x => OnButtonPressed());
        }
    }


    // Метод, который срабатывает при нажатии
    public void OnButtonPressed()
    {
        if (_isAnimating) return;

        // 1. Запускаем анимацию кнопки
        StartCoroutine(AnimateButton());

        // 2. Если сессия еще не активна — запускаем суд
        if (!isSessionActive)
        {
            if (firstSpeaker != null)
            {
                isSessionActive = true;
                Debug.Log("<color=green>[СУДЬЯ]: Заседание началось!</color>");
                firstSpeaker.StartDebate();
            }
            else
            {
                Debug.LogError("First Speaker не назначен в JudgeRoomManager!");
            }
        }
    }

    IEnumerator AnimateButton()
    {
        _isAnimating = true;
        Vector3 downPos = _startPos + new Vector3(0, -pushDepth, 0);

        // Вниз
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(_startPos, downPos, t);
            yield return null;
        }

        // Вверх
        t = 0;
        while (t < 1) {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(downPos, _startPos, t);
            yield return null;
        }

        transform.localPosition = _startPos;
        _isAnimating = false;
    }
}