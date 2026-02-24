using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class MyButton : MonoBehaviour
{
    public enum ColorType { Red, Yellow, Green, Blue }
    
    [Header("Setup")]
    public ColorType buttonColor; 
    public ProfessionTestManager gameManager; // Ссылка на твой основной скрипт

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
        
        if (_interactable != null)
            _interactable.selectEntered.AddListener(x => OnPress());
    }

    void OnPress()
    {
        if (_isPressed) return;
        
        if (gameManager != null)
            gameManager.AnswerChosen(buttonColor.ToString());

        StartCoroutine(AnimateButton());
    }

    IEnumerator AnimateButton()
    {
        _isPressed = true;
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
        _isPressed = false;
    }
}