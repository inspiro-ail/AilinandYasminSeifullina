using System;

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
    

    public ChatMessage(string r, string c)
    {
        role = r;
        content = c;
    }
    

    public ChatMessage()
    {
        role = "";
        content = "";
    }

}
