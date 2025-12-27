using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DebugAgentPrototype.Models;
using DebugAgentPrototype.Services;
using ReactiveUI;

namespace DebugAgentPrototype.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly AgentService _agentService;
    private readonly AppState _appState;
    private readonly LldbService _lldbService;
    private readonly OpenRouterService _openRouterService;
    private string _userInput = string.Empty;
    private string _lldbInput = string.Empty;
    private string _lldbOutput = string.Empty;
    private bool _isBusy;
    private bool _isLldbRunning;

    public ObservableCollection<ChatMessage> Messages { get; }

    public MainViewModel()
    {
        _appState = new AppState();
        _lldbService = new LldbService();
        _openRouterService = new OpenRouterService();
        _agentService = new AgentService(_lldbService, _openRouterService);

        _appState.Messages = _agentService.InitMessages();

        Messages = new ObservableCollection<ChatMessage>();

        // Subscribe to lldb output
        _lldbService.OutputReceived += OnLldbOutputReceived;

        // SendMessageCommand is enabled when not busy and user input is not empty
        var canSend = this.WhenAnyValue(
            x => x.IsBusy,
            x => x.UserInput,
            (busy, input) => !busy && !string.IsNullOrWhiteSpace(input));

        SendMessageCommand = ReactiveCommand.CreateFromTask(
            SendMessageAsync,
            canSend);

        // SendLldbCommand is enabled when lldb is running and input is not empty
        var canSendLldb = this.WhenAnyValue(
            x => x.IsLldbRunning,
            x => x.LldbInput,
            (running, input) => running && !string.IsNullOrWhiteSpace(input));

        SendLldbCommand = ReactiveCommand.CreateFromTask(
            SendLldbCommandAsync,
            canSendLldb);
    }

    public string LldbOutput
    {
        get => _lldbOutput;
        set => this.RaiseAndSetIfChanged(ref _lldbOutput, value);
    }

    public string LldbInput
    {
        get => _lldbInput;
        set => this.RaiseAndSetIfChanged(ref _lldbInput, value);
    }

    public string UserInput
    {
        get => _userInput;
        set => this.RaiseAndSetIfChanged(ref _userInput, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public bool IsLldbRunning
    {
        get => _isLldbRunning;
        set => this.RaiseAndSetIfChanged(ref _isLldbRunning, value);
    }

    public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
    public ReactiveCommand<Unit, Unit> SendLldbCommand { get; }

    private void OnLldbOutputReceived(object? sender, string output)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LldbOutput += output + "\n";
        });
    }

    private async Task SendLldbCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(LldbInput) || !_lldbService.IsRunning)
            return;

        var command = LldbInput.Trim();
        LldbInput = string.Empty; // Clear input immediately

        try
        {
            await _lldbService.SendCommandAsync(command, CancellationToken.None);
            // Update running state in case it changed
            Dispatcher.UIThread.Post(() =>
            {
                IsLldbRunning = _lldbService.IsRunning;
            });
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() =>
            {
                LldbOutput += $"Error: {ex.Message}\n";
                IsLldbRunning = _lldbService.IsRunning;
            });
        }
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput))
            return;

        var userText = UserInput.Trim();
        UserInput = string.Empty; 
        IsBusy = true;

        try
        {
            // Add user message immediately
            var userMessage = new ChatMessage
            {
                Role = ChatMessageRole.User,
                Text = userText
            };

            Dispatcher.UIThread.Post(() => Messages.Add(userMessage));

            // Process with agent
            var result = await _agentService.ProcessUserMessageAsync(userText, _appState, CancellationToken.None);

            // Update UI on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                // Build agent response text
                var responseText = result.AssistantReplyText;
                
                // Add tool calls information if present
                if (result.ToolCalls != null && result.ToolCalls.Count > 0)
                {
                    var toolCallsInfo = new StringBuilder();
                    toolCallsInfo.AppendLine("\n[Tool Calls]");
                    foreach (var toolCall in result.ToolCalls)
                    {
                        toolCallsInfo.AppendLine($"  • {toolCall.Name}({toolCall.Arguments})");
                    }
                    responseText += toolCallsInfo.ToString();
                }

                // Add agent response
                var agentMessage = new ChatMessage
                {
                    Role = ChatMessageRole.Agent,
                    Text = responseText
                };
                Messages.Add(agentMessage);

                // Update lldb running state
                IsLldbRunning = _lldbService.IsRunning;
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}

