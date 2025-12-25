using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
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
    private string _userInput = string.Empty;
    private bool _isBusy;

    public MainViewModel()
    {
        _appState = new AppState();
        _agentService = new AgentService(new DebuggerService());

        Messages = new ObservableCollection<ChatMessage>();
        CallStack = new ObservableCollection<StackFrame>();
        Breakpoints = new ObservableCollection<Breakpoint>();

        // SendMessageCommand is enabled when not busy and user input is not empty
        var canSend = this.WhenAnyValue(
            x => x.IsBusy,
            x => x.UserInput,
            (busy, input) => !busy && !string.IsNullOrWhiteSpace(input));

        SendMessageCommand = ReactiveCommand.CreateFromTask(
            SendMessageAsync,
            canSend);
    }

    public ObservableCollection<ChatMessage> Messages { get; }
    public ObservableCollection<StackFrame> CallStack { get; }
    public ObservableCollection<Breakpoint> Breakpoints { get; }

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

    public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput))
            return;

        var userText = UserInput.Trim();
        UserInput = string.Empty; // Clear input immediately
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
            var result = await _agentService.HandleAsync(userText, _appState, CancellationToken.None);

            // Update UI on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                // Add agent response
                var agentMessage = new ChatMessage
                {
                    Role = ChatMessageRole.Agent,
                    Text = result.AssistantReplyText
                };
                Messages.Add(agentMessage);

                // Handle breakpoint addition
                if (result.BreakpointAdded != null)
                {
                    Breakpoints.Add(result.BreakpointAdded);
                    // Keep AppState in sync
                    _appState.Breakpoints.Add(result.BreakpointAdded);
                }

                // Handle call stack update
                if (result.CallStack != null)
                {
                    CallStack.Clear();
                    foreach (var frame in result.CallStack)
                    {
                        CallStack.Add(frame);
                    }
                }
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}

