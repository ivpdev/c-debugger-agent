using System;
using System.Threading;
using System.Threading.Tasks;
using DebugAgentPrototype.Models;

namespace DebugAgentPrototype.Services;

public class AgentService
{
    private readonly DebuggerService _debuggerService;

    public AgentService(DebuggerService debuggerService)
    {
        _debuggerService = debuggerService;
    }

    public async Task<AgentResult> HandleAsync(string userText, AppState state, CancellationToken ct)
    {
        // Simulate async processing
        await Task.Delay(50, ct);

        var trimmed = userText.Trim();

        // Command: echo {whatever}
        if (trimmed.StartsWith("echo ", StringComparison.OrdinalIgnoreCase))
        {
            var content = trimmed.Substring(5); // Everything after "echo "
            return new AgentResult
            {
                AssistantReplyText = $"answer {content}"
            };
        }

        // Command: breakpoint {line}
        if (trimmed.StartsWith("breakpoint ", StringComparison.OrdinalIgnoreCase))
        {
            var lineStr = trimmed.Substring(11).Trim(); // Everything after "breakpoint "
            if (int.TryParse(lineStr, out int line) && line > 0)
            {
                var breakpoint = new Breakpoint(line);
                state.Breakpoints.Add(breakpoint);
                return new AgentResult
                {
                    AssistantReplyText = $"Breakpoint added at line {line}",
                    BreakpointAdded = breakpoint
                };
            }
            else
            {
                return new AgentResult
                {
                    AssistantReplyText = "Error: Invalid line number. Line must be a positive integer."
                };
            }
        }

        // Command: debug
        if (trimmed.Equals("debug", StringComparison.OrdinalIgnoreCase))
        {
            var debugOutput = await _debuggerService.RunAsync(state.Breakpoints, ct);
            return new AgentResult
            {
                AssistantReplyText = debugOutput.ConsoleOutput,
                CallStack = debugOutput.CallStack
            };
        }

        // Unknown command - return help
        return new AgentResult
        {
            AssistantReplyText = "Supported commands:\n" +
                                 "1. echo {text} - Echoes back the text\n" +
                                 "2. breakpoint {line} - Adds a breakpoint at the specified line\n" +
                                 "3. debug - Runs the debugger\n\n" +
                                 "Examples:\n" +
                                 "  echo hello world\n" +
                                 "  breakpoint 42\n" +
                                 "  debug"
        };
    }
}

