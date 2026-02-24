using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommandSequenceManager : MonoBehaviour
{
    public static CommandSequenceManager Instance;

    private List<CommandData> placedCommands = new List<CommandData>();
    public RobotController robot;

    private void Awake()
    {
        Instance = this;
    }

    public void AddCommand(string commandName, Vector3 position)
    {
        placedCommands.Add(new CommandData(commandName, position));
        Debug.Log($"🟩 Added command: {commandName} at {position}");
    }
    
    // --- NEW: Added a way to remove a command when it's picked up again ---
    public void RemoveCommandByTransform(Transform commandTransform)
    {
        int removedCount = placedCommands.RemoveAll(c => c.position == commandTransform.position);
        if (removedCount > 0)
        {
            Debug.Log($"🟧 Removed command at {commandTransform.position}");
        }
    }

    public void ClearCommands()
    {
        placedCommands.Clear();
        Debug.Log("🧹 Command list cleared.");
    }

    public void RunCommands()
    {
        if (robot == null)
        {
            Debug.LogWarning("⚠️ RobotController not assigned to CommandSequenceManager!");
            return;
        }

        // Sort by position.x. Make sure your board is oriented so this works.
        // For a vertical board, you might sort by position.y instead.
        var sorted = placedCommands.OrderBy(c => c.position.x).ToList();
        List<string> commands = sorted.Select(c => c.commandName).ToList();

        robot.ExecuteCommands(commands);
    }

    private class CommandData
    {
        public string commandName;
        public Vector3 position;

        public CommandData(string cmd, Vector3 pos)
        {
            commandName = cmd;
            position = pos;
        }
    }
}