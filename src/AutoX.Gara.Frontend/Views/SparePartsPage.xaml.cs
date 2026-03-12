// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class SparePartsPage : ContentPage
{
    public SparePartsPage()
    {
        InitializeComponent();
        BindingContext = new SparePartsViewModel(
            new SparePartService(
                InstanceManager.Instance.GetOrCreateInstance<SparePartQueryCache>()));
    }

    // OPT: Load data khi page thực sự hiện ra thay vì trong constructor.
    // Tránh race condition và popup lỗi xuất hiện trước khi UI render xong.
    // Parts.Count == 0 để không reload mỗi lần quay lại tab nếu đã có data.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SparePartsViewModel vm && vm.Parts.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }
}
