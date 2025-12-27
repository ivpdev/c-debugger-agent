using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DebugAgentPrototype.Models;

namespace DebugAgentPrototype.Services;

public class ToolsService
{
    public static List<ToolConfig> GetTools()
    {
        return new List<ToolConfig>
        {
            ToolRun.GetConfig(),
            ToolSetBreakpoint.GetConfig(),
            ToolGetSourceCode.GetConfig(),
            new ToolConfig("continue", "Continue the execution of the program", new { type = "object", properties = new { } })
            };
    }

    public static async Task<string> callTool(string toolName, string parameters, AppState state, LldbService lldbService, CancellationToken ct)
    {
        switch (toolName)
        {
            case "run": 
                return await ToolRun.CallAsync(state, lldbService, ct);
            case "breakpoint":
                return await ToolSetBreakpoint.CallAsync(parameters, state, lldbService, ct);
            case "get_source_code":
                return ToolGetSourceCode.CallAsync(state, lldbService, ct);
            case "continue":
                // TODO: Implement continue execution tool
                return "Execution continued";
            default:
                throw new Exception($"Tool {toolName} not found");
        }
    }

}

public class ToolConfig
{
    public string Name { get; }
    public string? Description { get; }
    public object Parameters { get; }

    public ToolConfig(string name, string description, object parameters)
    {
        Name = name;
        Description = description;
        Parameters = parameters;
    }
}

