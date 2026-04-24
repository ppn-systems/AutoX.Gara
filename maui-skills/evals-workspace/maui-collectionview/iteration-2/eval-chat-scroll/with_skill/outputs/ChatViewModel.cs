using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChatApp.Models;

namespace ChatApp.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    private string _inputText = string.Empty;

    public ObservableCollection<Message> Messages { get; } = new();

    public string InputText
    {
        get => _inputText;
        set
        {
            if (_inputText != value)
            {
                _inputText = value;
                OnPropertyChanged();
                ((Command)SendCommand).ChangeCanExecute();
            }
        }
    }

    public ICommand SendCommand { get; }

    // Event raised after a new message is added so the view can scroll to it.
    public event EventHandler<Message>? MessageAdded;

    public ChatViewModel()
    {
        SendCommand = new Command(
            execute: OnSend,
            canExecute: () => !string.IsNullOrWhiteSpace(InputText));

        // Seed a few starter messages so the UI is not empty on launch.
        Messages.Add(new Message
        {
            Text = "Hey, how are you?",
            SenderName = "Alice",
            IsFromMe = false,
            Timestamp = DateTime.Now.AddMinutes(-5)
        });
        Messages.Add(new Message
        {
            Text = "Doing great, thanks! How about you?",
            SenderName = "Me",
            IsFromMe = true,
            Timestamp = DateTime.Now.AddMinutes(-4)
        });
        Messages.Add(new Message
        {
            Text = "Pretty good! Did you see the game last night?",
            SenderName = "Alice",
            IsFromMe = false,
            Timestamp = DateTime.Now.AddMinutes(-3)
        });
    }

    private void OnSend()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        var message = new Message
        {
            Text = InputText.Trim(),
            SenderName = "Me",
            IsFromMe = true,
            Timestamp = DateTime.Now
        };

        // Always update the ObservableCollection on the UI thread.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(message);
            InputText = string.Empty;
            MessageAdded?.Invoke(this, message);
        });
    }

    // Simulate receiving an incoming message (useful for testing/demo).
    public void ReceiveMessage(string text, string senderName = "Alice")
    {
        var message = new Message
        {
            Text = text,
            SenderName = senderName,
            IsFromMe = false,
            Timestamp = DateTime.Now
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(message);
            MessageAdded?.Invoke(this, message);
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
