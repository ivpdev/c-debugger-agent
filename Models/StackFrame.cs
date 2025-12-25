namespace DebugAgentPrototype.Models;

public class StackFrame
{
    public string Function { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    
    public string DisplayText => $"{Function} ({File}:{Line})";
}

