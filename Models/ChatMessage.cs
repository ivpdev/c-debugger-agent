using System;

namespace DebugAgentPrototype.Models;

public class ChatMessage
{
    public ChatMessageRole Role { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public enum ChatMessageRole
{
    User,
    Agent,
    System,
}

