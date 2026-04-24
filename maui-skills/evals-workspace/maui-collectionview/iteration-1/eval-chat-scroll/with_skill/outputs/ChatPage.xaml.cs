using ChatApp.Models;
using ChatApp.ViewModels;

namespace ChatApp.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;

    public ChatPage()
    {
        InitializeComponent();
        _viewModel = new ChatViewModel();
        BindingContext = _viewModel;

        // Register the scroll callback so the view model can trigger auto-scroll
        // without needing a direct reference to the UI control.
        _viewModel.SetScrollToBottomCallback(ScrollToBottom);
    }

    /// <summary>
    /// Scrolls the CollectionView so the given message is visible at the end
    /// of the list (i.e. the bottom for a vertical list).
    /// </summary>
    private void ScrollToBottom(Message message)
    {
        // ScrollToPosition.End places the item at the bottom edge of the viewport.
        // animate: false gives instant snap for the first load; set true for
        // smooth animation on subsequent messages.
        MessagesCollectionView.ScrollTo(
            item: message,
            position: ScrollToPosition.End,
            animate: true);
    }
}
