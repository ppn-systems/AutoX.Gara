using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChatApp.Models;

namespace ChatApp.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    private string _newMessageText = string.Empty;
    private Action? _scrollToBottomAction;

    public ObservableCollection<Message> Messages { get; } = new();

    public string NewMessageText
    {
        get => _newMessageText;
        set
        {
            if (_newMessageText != value)
            {
                _newMessageText = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SendMessageCommand { get; }

    /// <summary>
    /// Register a callback that the View will hook up to trigger scroll-to-bottom.
    /// The ViewModel calls this action after adding a new message.
    /// </summary>
    public void RegisterScrollToBottomAction(Action action)
    {
        _scrollToBottomAction = action;
    }

    public ChatViewModel()
    {
        SendMessageCommand = new Command(OnSendMessage, CanSendMessage);

        // Seed with some sample messages so the UI has content to show.
        SeedMessages();
    }

    private bool CanSendMessage() => !string.IsNullOrWhiteSpace(NewMessageText);

    private void OnSendMessage()
    {
        if (string.IsNullOrWhiteSpace(NewMessageText))
            return;

        var message = new Message
        {
            Text = NewMessageText.Trim(),
            SenderName = "Me",
            IsMe = true,
            Timestamp = DateTime.Now
        };

        Messages.Add(message);
        NewMessageText = string.Empty;

        // Notify the View to scroll to the bottom.
        _scrollToBottomAction?.Invoke();

        // Simulate an incoming reply after a short delay.
        SimulateReplyAsync();
    }

    private async void SimulateReplyAsync()
    {
        await Task.Delay(1500);

        var reply = new Message
        {
            Text = "Got it! Thanks for the message.",
            SenderName = "Alice",
            IsMe = false,
            Timestamp = DateTime.Now
        };

        Messages.Add(reply);
        _scrollToBottomAction?.Invoke();
    }

    private void SeedMessages()
    {
        Messages.Add(new Message
        {
            Text = "Hey! How's it going?",
            SenderName = "Alice",
            IsMe = false,
            Timestamp = DateTime.Now.AddMinutes(-10)
        });

        Messages.Add(new Message
        {
            Text = "Pretty good, just working on a MAUI app.",
            SenderName = "Me",
            IsMe = true,
            Timestamp = DateTime.Now.AddMinutes(-9)
        });

        Messages.Add(new Message
        {
            Text = "Oh nice! What are you building?",
            SenderName = "Alice",
            IsMe = false,
            Timestamp = DateTime.Now.AddMinutes(-8)
        });

        Messages.Add(new Message
        {
            Text = "A chat app with auto-scroll. Almost done!",
            SenderName = "Me",
            IsMe = true,
            Timestamp = DateTime.Now.AddMinutes(-7)
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Keep the Send button state in sync when NewMessageText changes.
        if (propertyName == nameof(NewMessageText))
            (SendMessageCommand as Command)?.ChangeCanExecute();
    }
}
