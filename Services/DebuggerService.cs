using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DebugAgentPrototype.Models;

namespace DebugAgentPrototype.Services;

public class DebuggerService
{
    public async Task<DebugOutput> RunAsync(IReadOnlyList<Breakpoint> breakpoints, CancellationToken ct)
    {
        // Simulate async debugger operation
        await Task.Delay(100, ct);

        var output = new List<string>
        {
            "Attached to process 12345",
            "Debugger initialized",
            "Reading symbols..."
        };

        if (breakpoints.Count > 0)
        {
            var bpLines = string.Join(", ", breakpoints.Select(bp => bp.Line));
            output.Add($"Breakpoints: {bpLines}");
        }

        output.Add("Hit breakpoint at main (game.c:120)");
        output.Add("Execution paused");

        var callStack = new List<StackFrame>
        {
            new StackFrame { Function = "main", File = "game.c", Line = 120 },
            new StackFrame { Function = "play_game", File = "game.c", Line = 80 },
            new StackFrame { Function = "get_guess", File = "game.c", Line = 35 }
        };

        return new DebugOutput
        {
            ConsoleOutput = string.Join("\n", output),
            CallStack = callStack
        };
    }
}

