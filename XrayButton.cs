using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class XrayVRButton : MonoBehaviour
{
    [Header("Setup")]
    public XrayGameManager gameManager; // Ссылка на наш основной менеджер
    public bool isSickButton;           // Галка для "БОЛЕН", пустая для "ЗДОРОВ"

    [Header("Animation")]
    public float pushDepth = 0.02f;
    public float speed = 10f;

    private Vector3 _startPos;
    private bool _isPressed = false;
    private XRSimpleInteractable _interactable;

    void Start()
    {
        _startPos = transform.localPosition;
        _interactable = GetComponent<XRSimpleInteractable>();
        
        // Подписываемся на событие выбора (нажатие триггера или кнопки захвата)
        if (_interactable != null)
            _interactable.selectEntered.AddListener(x => OnPress());
    }

    void OnPress()
    {
        if (_isPressed) return;
        
        // Вызываем проверку ответа в нашем менеджере
        if (gameManager != null)
        {
            gameManager.ProcessAnswer(isSickButton);
        }

        StartCoroutine(AnimateButton());
    }

    IEnumerator AnimateButton()
    {
        _isPressed = true;
        Vector3 downPos = _startPos + new Vector3(0, -pushDepth, 0);

        // Движение вниз
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(_startPos, downPos, t);
            yield return null;
        }

        // Движение вверх
        t = 0;
        while (t < 1) {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(downPos, _startPos, t);
            yield return null;
        }

        transform.localPosition = _startPos;
        _isPressed = false;
    }
    
    // Хорошим тоном будет отписаться от события при уничтожении объекта
    private void OnDestroy()
    {
        if (_interactable != null)
            _interactable.selectEntered.RemoveAllListeners();
    }
}