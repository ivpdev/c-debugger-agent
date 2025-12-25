using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DebugAgentPrototype.Models;

namespace DebugAgentPrototype.Services;

public class AgentService
{
    private readonly LldbService _debuggerService;
    private readonly OpenRouterService _openRouterService;

    public AgentService(LldbService debuggerService, OpenRouterService llmService)
    {
        _debuggerService = debuggerService;
        _openRouterService = llmService;
    }

    public async Task<AgentResult> ProcessUserMessageAsync(string userText, AppState state, CancellationToken ct)
    {
        var userMessage = userText.Trim();

        // Command: echo {whatever}
        if (userMessage.StartsWith("echo ", StringComparison.OrdinalIgnoreCase))
        {
            return HandleEchoCommand(userMessage);
        }

        // Command: breakpoint {line}
        if (userMessage.StartsWith("breakpoint ", StringComparison.OrdinalIgnoreCase))
        {
            return HandleBreakpointCommand(userMessage, state);
        }

        // Command: debug
        if (userMessage.Equals("debug", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleDebugCommand(state, ct);
        }

        // Unknown command - return help
        return await HandleAnyCommandAsync(userMessage, state, ct);
    }

    private AgentResult HandleEchoCommand(string trimmed)
    {
        var content = trimmed.Substring(5); // Everything after "echo "
        return new AgentResult
        {
            AssistantReplyText = $"answer {content}"
        };
    }

    private AgentResult HandleBreakpointCommand(string trimmed, AppState state)
    {
        var lineStr = trimmed.Substring(11).Trim(); // Everything after "breakpoint "
        if (int.TryParse(lineStr, out int line) && line > 0)
        {
            var breakpoint = new Breakpoint(line);
            state.Breakpoints.Add(breakpoint);
            return new AgentResult
            {
                AssistantReplyText = $"Breakpoint added at line {line}"
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

    private async Task<AgentResult> HandleDebugCommand(AppState state, CancellationToken ct)
    {
        try
        {
            await _debuggerService.StartAsync(state.Breakpoints, ct);
            return new AgentResult
            {
                AssistantReplyText = "LLDB session started. Use the debugger panel to interact with lldb."
            };
        }
        catch (Exception ex)
        {
            return new AgentResult
            {
                AssistantReplyText = $"Error starting LLDB: {ex.Message}"
            };
        }
    }

    private async Task<AgentResult> HandleAnyCommandAsync(string userMessage, AppState state, CancellationToken ct) {
        var messages = addMesssageToHistory(userMessage, state.Messages);
        var tools = Tools.GetTools();
        // Convert List<object> to List<IMessage>
        var messageList = messages.Select(m => {
            // Use reflection to get role and content from anonymous objects
            var roleProp = m.GetType().GetProperty("role");
            var contentProp = m.GetType().GetProperty("content");
            return new Message(
                roleProp?.GetValue(m)?.ToString() ?? "user",
                contentProp?.GetValue(m)?.ToString() ?? ""
            ) as IMessage;
        }).ToList();
        var response = await _openRouterService.CallModelAsync(messageList, tools);
        if (response.ToolCalls.Count > 0) {
            foreach (var toolCall in response.ToolCalls) {
                var toolResult = await Tools.callTool(toolCall.Name, toolCall.Arguments, state, _debuggerService, ct);
            }
        }
        return new AgentResult 
        { 
            AssistantReplyText = response.Content,
            ToolCalls = response.ToolCalls
        };
    }

    public List<object> InitMessages()
    {
        const string systemPrompt = """
        You are a helpful assistant that can help with debugging a program.
        Feel free to use available tools.
        The user will provide a command and you will need to help them with it.
        Only call the tools needed for the most recent user message
        """;
        return new List<object> {
            new { role = "system", content = systemPrompt }
        };
    }

    private List<object> addMesssageToHistory(string message, List<object> history)
    {
        history.Add(new { role = "user", content = message });
        return history;
    }

    private class Message : IMessage
    {
        public string Role { get; }
        public string Content { get; }

        public Message(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}

