using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class JudgeRoomManager : MonoBehaviour
{
    [Header("Настройки Дебатов")]
    public DebateController firstSpeaker; 
    public bool isSessionActive = false;

    [Header("Анимация Кнопки")]
    public float pushDepth = 0.02f; 
    public float speed = 10f;      
    private Vector3 _startPos;
    private bool _isAnimating = false;

    private XRSimpleInteractable _interactable;

    void Start()
    {
        _startPos = transform.localPosition;
        _interactable = GetComponent<XRSimpleInteractable>();

        
        if (_interactable != null)
        {
            _interactable.selectEntered.AddListener(x => OnButtonPressed());
        }
    }



    public void OnButtonPressed()
    {
        if (_isAnimating) return;

        
        StartCoroutine(AnimateButton());

        
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
        _isAnimating = false;
    }

}
