using ChatApp.Models;
using ChatApp.ViewModels;
using Microsoft.Maui.Controls;

namespace ChatApp.Views
{
    public partial class ChatPage : ContentPage
    {
        private ChatViewModel? _viewModel;

        public ChatPage()
        {
            InitializeComponent();

            var vm = new ChatViewModel();
            BindingContext = vm;
            _viewModel = vm;

            // Subscribe to the MessageAdded event so we can scroll after
            // the CollectionView has laid out the new item.
            vm.MessageAdded += OnMessageAdded;
        }

        private void OnMessageAdded(object? sender, Message message)
        {
            // ScrollTo must run on the main thread; ViewModel callbacks from
            // async continuations (e.g. the simulated reply) arrive on a
            // thread-pool thread, so we dispatch to main explicitly.
            MainThread.BeginInvokeOnMainThread(() => ScrollToLastMessage());
        }

        private void ScrollToLastMessage()
        {
            if (_viewModel is null || _viewModel.Messages.Count == 0)
                return;

            var lastMessage = _viewModel.Messages[^1];

            // animate: true gives a smooth slide; pass ScrollToPosition.End so
            // the item is fully visible at the bottom of the viewport.
            MessagesCollectionView.ScrollTo(
                item: lastMessage,
                position: ScrollToPosition.End,
                animate: true);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Scroll to the bottom when the page first appears so the most
            // recent pre-loaded messages are visible without user interaction.
            ScrollToLastMessage();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe to prevent memory leaks when the page is removed
            // from the navigation stack.
            if (_viewModel is not null)
                _viewModel.MessageAdded -= OnMessageAdded;
        }
    }
}
