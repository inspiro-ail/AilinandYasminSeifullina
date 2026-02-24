using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class CommandButtonSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform wallPlane;
    public GameObject buttonPrefab;
    public string commandName;

    [Header("Wall Constraint Settings")]
    public float wallDistance = 0.01f;


    public bool IsClone { get; private set; } = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool hasSpawnedClone = false;
    private Quaternion originalRotation;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        originalRotation = transform.rotation;
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!IsClone && !hasSpawnedClone)
        {
            hasSpawnedClone = true;
            
            GameObject clone = Instantiate(buttonPrefab, transform.position, transform.rotation);
            CommandButtonSpawner cloneScript = clone.GetComponent<CommandButtonSpawner>();
            cloneScript.wallPlane = wallPlane;
            cloneScript.IsClone = true; 
            cloneScript.commandName = commandName;
            cloneScript.buttonPrefab = buttonPrefab;
            cloneScript.originalRotation = transform.rotation;
            
            cloneScript.grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
            cloneScript.grabInteractable.trackPosition = false;
            cloneScript.grabInteractable.trackRotation = false;

            grabInteractable.interactionManager.SelectExit(args.interactorObject, grabInteractable);
            grabInteractable.interactionManager.SelectEnter(args.interactorObject, cloneScript.grabInteractable);
        }
        else if (IsClone)
        {
            
            CommandSequenceManager.Instance?.RemoveCommandByTransform(transform);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (!IsClone)
        {
            hasSpawnedClone = false;
        }
        
        if (IsClone)
        {
            
            CommandSequenceManager.Instance?.AddCommand(commandName, transform.position);
        }
    }

    private void LateUpdate()
    {
        if (IsClone && grabInteractable.isSelected && wallPlane != null)
        {
            var interactor = grabInteractable.interactorsSelecting[0].transform;
            ConstrainToWall(interactor.position);
        }
    }

    private void ConstrainToWall(Vector3 targetPosition)
    {
        if (wallPlane == null) return;

        Plane plane = new Plane(-wallPlane.forward, wallPlane.position);
        Vector3 projectedPos = plane.ClosestPointOnPlane(targetPosition);
        
        transform.position = projectedPos + wallPlane.forward * wallDistance;
        transform.rotation = originalRotation;
    }

}
