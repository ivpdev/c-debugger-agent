using System.Collections.Generic;

namespace DebugAgentPrototype.Models;

public class AppState
{
    public List<Breakpoint> Breakpoints { get; set; } = new();
    public List<StackFrame> CurrentCallStack { get; set; } = new();
    public List<object> Messages { get; set; } = new();
}

