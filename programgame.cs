using UnityEngine;
using UnityEngine.InputSystem;
public class programgame : MonoBehaviour
{
    public GameObject forward;
    public GameObject back;
    public GameObject right;
    public GameObject left;
    public InputActionProperty triggerAction;
    void Start()
    {
        if (triggerAction.action.WasPressedThisFrame())
        {
            Debug.Log("Триггер нажат!");
        }
    }
    void Update()
    {
        if (triggerAction.action.WasPressedThisFrame())
        {
            Debug.Log("Триггер нажат!");
        }
    }
}
