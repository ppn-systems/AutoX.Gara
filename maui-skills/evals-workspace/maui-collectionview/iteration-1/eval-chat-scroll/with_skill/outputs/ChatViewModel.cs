using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChatApp.Models;

namespace ChatApp.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    private string _messageText = string.Empty;
    private Action<Message>? _scrollToBottomCallback;

    public ObservableCollection<Message> Messages { get; } = new();

    public string MessageText
    {
        get => _messageText;
        set
        {
            if (_messageText != value)
            {
                _messageText = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SendCommand { get; }
    public ICommand SimulateIncomingCommand { get; }

    public ChatViewModel()
    {
        SendCommand = new Command(OnSend, () => !string.IsNullOrWhiteSpace(MessageText));
        SimulateIncomingCommand = new Command(OnSimulateIncoming);

        // Watch for new items so we can auto-scroll
        Messages.CollectionChanged += OnMessagesCollectionChanged;

        // Seed a few starter messages
        Messages.Add(new Message
        {
            Text = "Hey! How are you?",
            SenderName = "Alice",
            Timestamp = DateTime.Now.AddMinutes(-5),
            IsFromMe = false
        });
        Messages.Add(new Message
        {
            Text = "Doing great, thanks! You?",
            SenderName = "Me",
            Timestamp = DateTime.Now.AddMinutes(-4),
            IsFromMe = true
        });
        Messages.Add(new Message
        {
            Text = "Pretty good. Did you see the game last night?",
            SenderName = "Alice",
            Timestamp = DateTime.Now.AddMinutes(-3),
            IsFromMe = false
        });
    }

    /// <summary>
    /// Register the scroll callback from the view (code-behind).
    /// The view passes a delegate that calls collectionView.ScrollTo(...).
    /// </summary>
    public void SetScrollToBottomCallback(Action<Message> callback)
    {
        _scrollToBottomCallback = callback;
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            var newMessage = (Message)e.NewItems[e.NewItems.Count - 1]!;
            // Must scroll on the UI thread
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                _scrollToBottomCallback?.Invoke(newMessage);
            });
        }
    }

    private void OnSend()
    {
        if (string.IsNullOrWhiteSpace(MessageText))
            return;

        Messages.Add(new Message
        {
            Text = MessageText.Trim(),
            SenderName = "Me",
            Timestamp = DateTime.Now,
            IsFromMe = true
        });

        MessageText = string.Empty;
    }

    private void OnSimulateIncoming()
    {
        var samples = new[]
        {
            "That sounds great!",
            "Let me think about it.",
            "Sure, I'll be there.",
            "Can you send me the details?",
            "Sounds like a plan!"
        };

        var rng = new Random();
        Messages.Add(new Message
        {
            Text = samples[rng.Next(samples.Length)],
            SenderName = "Alice",
            Timestamp = DateTime.Now,
            IsFromMe = false
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Re-evaluate CanExecute for SendCommand when MessageText changes
        if (name == nameof(MessageText))
            (SendCommand as Command)?.ChangeCanExecute();
    }
}
