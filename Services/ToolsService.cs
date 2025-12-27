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
            new ToolConfig("run", "Run the program", new { type = "object", properties = new { } }),
            new ToolConfig("breakpoint", "Set a breakpoint at the given line number", new { 
                type = "object",
                properties = new {
                    line = new { 
                        type = "integer", 
                        description = "Line number where to set the breakpoint" 
                    }
                },
                required = new[] { "line" }
            }),
            new ToolConfig("continue", "Continue the execution of the program", new { type = "object", properties = new { } })
        };
    }

    public static async Task<string> callTool(string toolName, string parameters, AppState state, LldbService lldbService, CancellationToken ct)
    {
        switch (toolName)
        {
            case "run": 
                await lldbService.StartAsync(state.Breakpoints, ct);

                return "Program run";
            case "breakpoint":
                if (string.IsNullOrWhiteSpace(parameters))
                {
                    throw new ArgumentException("Parameters cannot be empty for breakpoint tool");
                }

                JsonDocument? jsonDoc = null;
                try
                {
                    jsonDoc = JsonDocument.Parse(parameters);
                    var root = jsonDoc.RootElement;

                    if (!root.TryGetProperty("line", out var lineElement))
                    {
                        throw new ArgumentException("Missing required 'line' parameter");
                    }

                    if (!lineElement.TryGetInt32(out int line) || line <= 0)
                    {
                        throw new ArgumentException("'line' must be a positive integer");
                    }

                    state.Breakpoints.Add(new Breakpoint(line));

                    if (lldbService.IsRunning)
                    {
                        await lldbService.SendCommandAsync($"breakpoint set --file game.c --line {line}", ct);
                    }

                    return $"Breakpoint set at line {line}";
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid JSON parameters: {ex.Message}", ex);
                }
                finally
                {
                    jsonDoc?.Dispose();
                }
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
