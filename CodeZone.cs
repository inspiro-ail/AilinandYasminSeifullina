using System.Collections.Generic;
using UnityEngine;

public class CodeZone : MonoBehaviour
{
    public Transform visualParent; 
    public GameObject commandPrefabClone; 
    public float slotSpacing = 0.12f;

    [HideInInspector]
    public List<string> commandList = new List<string>();

    private int count = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("CommandBlock")) return;


        var tmp = other.GetComponentInChildren<TMPro.TextMeshPro>();
        string commandText = tmp != null ? tmp.text : other.name;


        commandList.Add(commandText);


        if (commandPrefabClone != null && visualParent != null)
        {
            Vector3 slotPos = visualParent.position + visualParent.right * (count * slotSpacing);
            GameObject clone = Instantiate(commandPrefabClone, slotPos, Quaternion.identity, visualParent);

            var cloneTmp = clone.GetComponentInChildren<TMPro.TextMeshPro>();
            if (cloneTmp != null) cloneTmp.text = commandText;

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
