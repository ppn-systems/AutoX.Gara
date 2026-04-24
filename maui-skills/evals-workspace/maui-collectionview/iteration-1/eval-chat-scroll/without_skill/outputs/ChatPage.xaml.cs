using ChatApp.Models;
using ChatApp.ViewModels;

namespace ChatApp.Views;

public partial class ChatPage : ContentPage
{
    private ChatViewModel? _viewModel;

    public ChatPage()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        // Unsubscribe from the previous ViewModel if one existed.
        if (_viewModel is not null)
        {
            _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
        }

        _viewModel = BindingContext as ChatViewModel;

        if (_viewModel is not null)
        {
            // Register the scroll-to-bottom delegate so the ViewModel can
            // request a scroll without depending on any UI types.
            _viewModel.RegisterScrollToBottomAction(ScrollToBottom);

            // Also listen to CollectionChanged so we scroll whenever
            // a message is added externally (e.g. push notifications).
            _viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Scroll to the bottom when the page first appears so the
        // user sees the most-recent messages immediately.
        if (_viewModel?.Messages.Count > 0)
        {
            // Small delay to allow the CollectionView to finish its
            // initial layout before we try to scroll.
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), ScrollToBottom);
        }
    }

    private void OnMessagesCollectionChanged(
        object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Only scroll when items are added (not removed or reset).
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            ScrollToBottom();
        }
    }

    private void ScrollToBottom()
    {
        if (_viewModel is null || _viewModel.Messages.Count == 0)
            return;

        var lastMessage = _viewModel.Messages[^1];

        // ScrollTo must run on the main thread.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MessagesCollectionView.ScrollTo(lastMessage, position: ScrollToPosition.End, animate: true);
        });
    }
}
