using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class VRStartButton : MonoBehaviour
{
    [Header("References")]
    public CommandSequenceManager commandManager;
    public Renderer buttonRenderer;
    public AudioSource clickSound;

    [Header("Feedback Settings")]
    public Color pressedColor = Color.green;
    public float pressDepth = 0.02f;
    public float resetSpeed = 5f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private Vector3 originalPos;
    private Color defaultColor;
    private bool isPressed = false;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        originalPos = transform.localPosition;

        if (buttonRenderer != null)
            defaultColor = buttonRenderer.material.color;


        interactable.selectEntered.AddListener(OnSelectPressed);
        interactable.activated.AddListener(OnActivated);


        interactable.hoverEntered.AddListener(_ => Debug.Log("[StartButton] hoverEntered"));
        interactable.hoverExited.AddListener(_ => Debug.Log("[StartButton] hoverExited"));
        interactable.selectExited.AddListener(_ => Debug.Log("[StartButton] selectExited"));
        interactable.deactivated.AddListener(_ => Debug.Log("[StartButton] deactivated"));
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnSelectPressed);
        interactable.activated.RemoveListener(OnActivated);
        interactable.hoverEntered.RemoveAllListeners();
        interactable.hoverExited.RemoveAllListeners();
        interactable.selectExited.RemoveAllListeners();
        interactable.deactivated.RemoveAllListeners();
    }

    private void Update()
    {
        if (!isPressed)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, Time.deltaTime * resetSpeed);
            if (buttonRenderer != null)
                buttonRenderer.material.color = Color.Lerp(buttonRenderer.material.color, defaultColor, Time.deltaTime * resetSpeed);
        }
    }

    private void OnSelectPressed(SelectEnterEventArgs args)
    {
        Debug.Log("[StartButton] selectEntered");
        Press();
    }

    private void OnActivated(ActivateEventArgs args)
    {
        Debug.Log("[StartButton] activated");
        Press();
    }

    private void Press()
    {
        if (isPressed) return;
        isPressed = true;

        Debug.Log("🚀 START BUTTON PRESSED");

        if (clickSound != null) clickSound.Play();
        if (buttonRenderer != null) buttonRenderer.material.color = pressedColor;
        transform.localPosition = originalPos - transform.up * pressDepth;

        if (commandManager != null)
        {
            commandManager.RunCommands();
            commandManager.ClearCommands(); 
        }
        else
        {
            Debug.LogError("❌ CommandSequenceManager not assigned in StartButton!");
        }

        RemoveAllClones();

        Invoke(nameof(ResetButton), 0.4f);
    }

    private void ResetButton()
    {
        isPressed = false;
        if (buttonRenderer != null)
            buttonRenderer.material.color = defaultColor;
    }

    private void RemoveAllClones()
    {
        var allButtons = FindObjectsOfType<CommandButtonSpawner>();
        int removed = 0;
        foreach (var button in allButtons)
        {
            if (button != null && button.isActiveAndEnabled && button.IsClone)
            {
                Destroy(button.gameObject);
                removed++;
            }
        }
        Debug.Log($"🧹 Removed {removed} clone buttons after Start pressed.");
    }

}
