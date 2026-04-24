using ChatApp.Models;
using ChatApp.ViewModels;

namespace ChatApp.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Subscribe to the ViewModel event instead of CollectionChanged so
        // we always have the concrete item reference needed by ScrollTo(item).
        _viewModel.MessageAdded += OnMessageAdded;

        // Also hook CollectionChanged to handle messages that arrive without
        // going through the ViewModel's SendCommand (e.g. from a service push).
        _viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ScrollToLastMessage(animate: false);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.MessageAdded -= OnMessageAdded;
        _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
    }

    // Called by the ViewModel event — carries the exact new Message object.
    private void OnMessageAdded(object? sender, Message message)
    {
        ScrollToMessage(message, animate: true);
    }

    // Fallback: handles CollectionChanged for external inserts (e.g. a background
    // message service adds directly to Messages without raising MessageAdded).
    private void OnMessagesCollectionChanged(
        object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            return;

        // Only scroll when an item was not already handled by OnMessageAdded.
        // The MessageAdded event fires before CollectionChanged, so if the new
        // item came from SendCommand we'd scroll twice; guard against that by
        // checking whether the added item is the same reference already handled.
        // For simplicity here we just scroll to the last item unconditionally —
        // ScrollTo on an already-visible item is a no-op on most platforms.
        ScrollToLastMessage(animate: true);
    }

    private void ScrollToLastMessage(bool animate)
    {
        var messages = _viewModel.Messages;
        if (messages.Count == 0)
            return;

        var last = messages[messages.Count - 1];
        ScrollToMessage(last, animate);
    }

    private void ScrollToMessage(Message message, bool animate)
    {
        // Must run on the UI thread; callers may arrive from a background context.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // ScrollToPosition.End places the item at the bottom of the viewport,
            // matching the expected chat behaviour where the newest message appears
            // at the bottom.
            MessagesCollectionView.ScrollTo(
                item: message,
                position: ScrollToPosition.End,
                animate: animate);
        });
    }
}
