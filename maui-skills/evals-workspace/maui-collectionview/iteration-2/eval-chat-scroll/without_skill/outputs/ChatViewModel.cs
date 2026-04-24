using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChatApp.Models;

namespace ChatApp.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private string _newMessageText = string.Empty;

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public string NewMessageText
        {
            get => _newMessageText;
            set
            {
                if (_newMessageText != value)
                {
                    _newMessageText = value;
                    OnPropertyChanged();
                    ((Command)SendMessageCommand).ChangeCanExecute();
                }
            }
        }

        public ICommand SendMessageCommand { get; }

        // Event raised when a new message is added; subscribers can scroll to it.
        public event EventHandler<Message>? MessageAdded;

        public ChatViewModel()
        {
            SendMessageCommand = new Command(
                execute: SendMessage,
                canExecute: () => !string.IsNullOrWhiteSpace(NewMessageText));

            // Seed some initial conversation so the UI is not empty.
            Messages.Add(new Message
            {
                Text = "Hey! Are you coming to the meeting today?",
                IsFromMe = false,
                SenderName = "Alice",
                Timestamp = DateTime.Now.AddMinutes(-10)
            });
            Messages.Add(new Message
            {
                Text = "Yes, I'll be there at 2 PM.",
                IsFromMe = true,
                SenderName = "Me",
                Timestamp = DateTime.Now.AddMinutes(-9)
            });
            Messages.Add(new Message
            {
                Text = "Great, see you then!",
                IsFromMe = false,
                SenderName = "Alice",
                Timestamp = DateTime.Now.AddMinutes(-8)
            });
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(NewMessageText))
                return;

            var message = new Message
            {
                Text = NewMessageText.Trim(),
                IsFromMe = true,
                SenderName = "Me",
                Timestamp = DateTime.Now
            };

            Messages.Add(message);
            NewMessageText = string.Empty;

            MessageAdded?.Invoke(this, message);

            // Simulate a reply after a short delay.
            SimulateReply();
        }

        private async void SimulateReply()
        {
            await Task.Delay(1500);

            var reply = new Message
            {
                Text = "Got it! Thanks for the update.",
                IsFromMe = false,
                SenderName = "Alice",
                Timestamp = DateTime.Now
            };

            Messages.Add(reply);
            MessageAdded?.Invoke(this, reply);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
