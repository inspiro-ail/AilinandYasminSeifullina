using System.Collections.Generic;
using UnityEngine;

public class CodeZone : MonoBehaviour
{
    public Transform visualParent; // CommandsHolder — куда ставим визуальную копию
    public GameObject commandPrefabClone; // можно использовать тот же префаб или простой визуал
    public float slotSpacing = 0.12f;

    [HideInInspector]
    public List<string> commandList = new List<string>();

    private int count = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("CommandBlock")) return;

        // Определяем команду по тексту внутри
        var tmp = other.GetComponentInChildren<TMPro.TextMeshPro>();
        string commandText = tmp != null ? tmp.text : other.name;

        // Добавляем в список (логика выполнения)
        commandList.Add(commandText);

        // Визуальная копия: клонируем префаб (можно сделать облегчённую копию)
        if (commandPrefabClone != null && visualParent != null)
        {
            Vector3 slotPos = visualParent.position + visualParent.right * (count * slotSpacing);
            GameObject clone = Instantiate(commandPrefabClone, slotPos, Quaternion.identity, visualParent);
            // Установим текст на клоне
            var cloneTmp = clone.GetComponentInChildren<TMPro.TextMeshPro>();
            if (cloneTmp != null) cloneTmp.text = commandText;
            // Отключаем возможность захвата у клона
            var grab = clone.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null) Destroy(grab);
            var rb = clone.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);
            count++;
        }

        Debug.Log("Добавлена команда: " + commandText);
    }

    public void ClearCommands()
    {
        commandList.Clear();
        count = 0;
        if (visualParent != null)
        {
            for (int i = visualParent.childCount - 1; i >= 0; i--)
                Destroy(visualParent.GetChild(i).gameObject);
        }
    }
}