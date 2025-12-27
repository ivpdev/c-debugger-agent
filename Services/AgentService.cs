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

        var assistantMessage = toAssistantMessage(response);
        state.Messages.Add(assistantMessage);

        if (assistantMessage.ToolCallRequests.Count > 0) {
            var toolCalls = await ToolsService.callToolsAsync(assistantMessage.ToolCallRequests, state, _lldbService, ct);
            state.Messages.Add(new ToolCallMessage { ToolCalls = toolCalls });
        }
        
        return new AgentResult 
        { 
            AssistantReplyText = response.Content,
            ToolCalls = response.ToolCalls
        };
    }

    private void PrintMessageHistory(List<ChatMessage> messages)
    {
        Console.WriteLine("=== Full Message History ===");
        foreach (var msg in messages)
        {
            Console.WriteLine($"[{msg.Role}] {msg.Text}");
            if (msg is AssistantMessage am && am.ToolCallRequests.Count > 0)
            {
                foreach (var toolCall in am.ToolCallRequests)
                {
                    Console.WriteLine($"  Tool Call: {toolCall.Name}({toolCall.Arguments})");
                }
            }
            if (msg is ToolCallMessage tm && tm.ToolCalls.Count > 0)
            {
                foreach (var toolCall in tm.ToolCalls)
                {
                    Console.WriteLine($"  Tool Result: {toolCall.Name} -> {toolCall.Result}");
                }
            }
        }
        Console.WriteLine("===========================");
    }

    private AssistantMessage toAssistantMessage(ILlmResponse response)
    {
        var toolCallRequests = response.ToolCalls.Select(tc => new ToolCallRequest
        {
            Id = tc.Id,
            Name = tc.Name,
            Arguments = tc.Arguments
        }).ToList();
        
        return new AssistantMessage 
        { 
            Text = response.Content,
            ToolCallRequests = toolCallRequests
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

