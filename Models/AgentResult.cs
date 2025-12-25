using System.Collections.Generic;

namespace DebugAgentPrototype.Models;

public class AgentResult
{
    public string AssistantReplyText { get; set; } = string.Empty;
    public Breakpoint? BreakpointAdded { get; set; }
    public List<StackFrame>? CallStack { get; set; }
}

