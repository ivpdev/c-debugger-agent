using System.Collections.Generic;

namespace DebugAgentPrototype.Models;

public class DebugOutput
{
    public string ConsoleOutput { get; set; } = string.Empty;
    public List<StackFrame> CallStack { get; set; } = new();
}

