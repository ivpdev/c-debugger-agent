using System.Collections.Generic;

namespace DebugAgentPrototype.Models;

public class AppState
{
    public List<Breakpoint> Breakpoints { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
}

