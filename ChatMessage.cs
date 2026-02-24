using System;

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
    
    // Constructor with parameters
    public ChatMessage(string r, string c)
    {
        role = r;
        content = c;
    }
    
    // Default constructor for JSON
    public ChatMessage()
    {
        role = "";
        content = "";
    }
}