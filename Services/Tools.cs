using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DebugAgentPrototype.Models;

namespace DebugAgentPrototype.Services;

public class Tools
{
    public static List<IToolConfig> GetTools()
    {
        return new List<IToolConfig>
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

    public static Task<string> callTool(string toolName, string parameters)
    {
        switch (toolName)
        {
            case "run":
                // TODO: Implement run program tool 
                return Task.FromResult("Program run");
            case "breakpoint":
                // TODO: Implement breakpoint tool
                return Task.FromResult("Breakpoint set");
            case "continue":
                // TODO: Implement continue execution tool
                return Task.FromResult("Execution continued");
            default:
                throw new Exception($"Tool {toolName} not found");
        }
    }

    private class ToolConfig : IToolConfig
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
}
