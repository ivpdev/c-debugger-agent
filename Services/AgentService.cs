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
    private readonly LldbService _lldbService;
    private readonly OpenRouterService _openRouterService;

    public AgentService(LldbService lldbService, OpenRouterService llmService)
    {
        _lldbService = lldbService;
        _openRouterService = llmService;
    }

    public async Task<AgentResult> ProcessUserMessageAsync(string userText, AppState state, CancellationToken ct)
    {
        var userMessage = userText.Trim();
        return await HandleAnyCommandAsync(userMessage, state, ct);
    }

    private async Task<AgentResult> HandleAnyCommandAsync(string userMessage, AppState state, CancellationToken ct) {
        state.Messages.Add(new ChatMessage { Role = ChatMessageRole.User, Text = userMessage });
        var tools = ToolsService.GetTools();
        var response = await _openRouterService.CallModelAsync(state.Messages, tools);
        if (response.ToolCalls.Count > 0) {
            foreach (var toolCall in response.ToolCalls) {
                var toolResult = await ToolsService.callTool(toolCall.Name, toolCall.Arguments, state, _lldbService, ct);
            }
        }
        return new AgentResult 
        { 
            AssistantReplyText = response.Content,
            ToolCalls = response.ToolCalls
        };
    }

    public List<ChatMessage> InitMessages()
    {
        const string systemPrompt = """
        You are a helpful assistant that can help with debugging a program.
        Feel free to use available tools.
        The user will provide a command and you will need to help them with it.
        Only call the tools needed for the most recent user message
        """;
        return new List<ChatMessage> {
            new ChatMessage { Role = ChatMessageRole.System, Text = systemPrompt }
        };
    }

}

