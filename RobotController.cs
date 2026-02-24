using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 1f;
    public float moveSpeed = 1f;       
    public float rotationSpeed = 90f;  

    [Header("Collision Settings")]
    public LayerMask obstacleMask = ~0;   
    public float collisionRadius = 0.25f; 
    public float castHeight = 0.25f;      
    public float skin = 0.02f;            

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    [SerializeField] private BoardUI boardUI; 

    public void ExecuteCommands(List<string> commands)
    {
        StopAllCoroutines();
        StartCoroutine(Execute(commands));
    }

    private IEnumerator Execute(List<string> commands)
    {
        foreach (string cmd in commands)
        {
            if (cmd == "forward")
                yield return MoveInDirection(transform.forward);
            else if (cmd == "left")
                yield return Turn(-90);
            else if (cmd == "right")
                yield return Turn(90);
            else if (cmd == "back")
                yield return MoveInDirection(-transform.forward);

            if (CheckIfOnExit())
            {
                Debug.Log("✅ Robot reached the goal!");
                boardUI?.ShowWin("You did it!");
                PlayAnimation("Idle");
                yield break;
            }
        }

        Debug.Log("❌ Robot did not reach the goal.");
        PlayAnimation("Idle");
    }

    private IEnumerator MoveInDirection(Vector3 dir)
    {
        float allowed = ComputeFreeDistance(dir, moveDistance);
        if (allowed <= 0.001f)
        {
            Debug.Log("🧱 Move blocked by wall.");
            PlayAnimation("Idle");
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        PlayAnimation("Walk");

        Vector3 start = transform.position;
        Vector3 end = start + dir.normalized * allowed;

        float t = 0f;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(start, end, t);


            if (CheckIfOnExit())
            {
                Debug.Log("✅ Robot reached the goal (mid-move)!");
                boardUI?.ShowWin("You did it!");
                PlayAnimation("Idle");
                yield break;
            }

            t += Time.deltaTime * moveSpeed;
            yield return null;
        }
        transform.position = end;

        PlayAnimation("Idle");
    }

    private float ComputeFreeDistance(Vector3 dir, float maxDistance)
    {
        Vector3 origin = transform.position + Vector3.up * castHeight;

        if (Physics.SphereCast(origin, collisionRadius, dir.normalized,
                               out RaycastHit hit, maxDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            float dist = Mathf.Max(0f, hit.distance - skin);
            return dist;
        }
        return maxDistance;
    }

    private IEnumerator Turn(float angle)
    {
        PlayAnimation("Idle");

        Quaternion start = transform.rotation;
        Quaternion end = start * Quaternion.Euler(0, angle, 0);
        float t = 0f;
        while (t < 1f)
        {
            transform.rotation = Quaternion.Slerp(start, end, t);
            t += Time.deltaTime * rotationSpeed / 90f;
            yield return null;
        }
        transform.rotation = end;
    }

    private bool CheckIfOnExit()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, 0.15f, ~0, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Exit"))
                return true;
        }
        return false;
    }

    private void PlayAnimation(string animName)
    {
        if (animator != null) animator.CrossFade(animName, 0.1f);
    }

}
