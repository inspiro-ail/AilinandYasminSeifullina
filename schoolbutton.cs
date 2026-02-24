using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class LessonManager : MonoBehaviour
{
    public bool isLessonStarted = false;
    public StudentController mainStudent; 

    [Header("Animation")]
    public float pushDepth = 0.02f;
    public float speed = 10f;
    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.localPosition;
        
        // Автоматическая подписка на событие XR кнопки
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(x => StartLesson());
        }
    }

    public void StartLesson()
    {
        if (isLessonStarted) return;
        
        isLessonStarted = true;
        Debug.Log("<color=green>[СИСТЕМА]: Урок начался!</color>");
        
        // Визуальное нажатие
        StartCoroutine(AnimateButton());
        
        // Уведомляем ученика
        if (mainStudent != null) 
            mainStudent.PlayInitialAnimation(); 
    }

    IEnumerator AnimateButton()
    {
        Vector3 downPos = _startPos + new Vector3(0, -pushDepth, 0);
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(_startPos, downPos, t);
            yield return null;
        }
        t = 0;
        while (t < 1) {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(downPos, _startPos, t);
            yield return null;
        }
        transform.localPosition = _startPos;
    }
}