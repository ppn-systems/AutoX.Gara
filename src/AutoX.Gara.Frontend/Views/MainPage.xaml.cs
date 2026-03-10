// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.Accessibility;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend.Views;

public partial class MainPage : ContentPage
{
    private System.Int32 count = 0;

    public MainPage() => InitializeComponent();

    private void OnCounterClicked(System.Object? sender, System.EventArgs e)
    {
        count++;

        CounterBtn.Text = count == 1 ? $"Clicked {count} time" : $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
