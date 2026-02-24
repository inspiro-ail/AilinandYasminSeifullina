using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class RoomActivator : MonoBehaviour
{
    [Header("Связь с Психологом")]
    public VoiceAIController patientController; // Сюда перетащи объект с VoiceAIController
    public bool isSessionActive = false;

    [Header("Анимация Кнопки")]
    public float pushDepth = 0.02f; 
    public float speed = 10f;       
    private Vector3 _startPos;
    private bool _isAnimating = false;

    private XRSimpleInteractable _interactable;

    void Start()
    {
        // Сохраняем начальную позицию для анимации
        _startPos = transform.localPosition;
        
        // Пытаемся найти компонент взаимодействия на этом же объекте
        _interactable = GetComponent<XRSimpleInteractable>();

        if (_interactable != null)
        {
            // Подписываем метод активации на событие Select Entered (нажатие триггера)
            _interactable.selectEntered.AddListener(x => OnButtonPressed());
        }
        else
        {
            Debug.LogError("На кнопке отсутствует XRSimpleInteractable!");
        }
    }

    public void OnButtonPressed()
    {
        if (_isAnimating) return;

        // 1. Запускаем анимацию (визуальный отклик)
        StartCoroutine(AnimateButton());

        // 2. Активируем логику комнаты, если она еще не запущена
        if (!isSessionActive)
        {
            if (patientController != null)
            {
                isSessionActive = true;
                
                // ВАЖНО: вызываем метод активации в твоем VoiceAIController
                patientController.ActivateRoom(); 
                
                Debug.Log("<color=cyan>[СИСТЕМА]: Сеанс психотерапии начат кнопкой!</color>");
            }
            else
            {
                Debug.LogError("Patient Controller не назначен в PsychologistRoomManager!");
            }
        }
    }

    IEnumerator AnimateButton()
    {
        _isAnimating = true;
        Vector3 downPos = _startPos + new Vector3(0, -pushDepth, 0);

        // Движение вниз
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(_startPos, downPos, t);
            yield return null;
        }

        // Небольшая пауза в нажатом состоянии
        yield return new WaitForSeconds(0.05f);

        // Движение вверх
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