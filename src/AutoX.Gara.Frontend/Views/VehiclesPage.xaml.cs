// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Vehicles;
using AutoX.Gara.Frontend.ViewModels;
using AutoX.Gara.Shared.Protocol.Customers;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Code-behind của VehiclesPage.
/// <para>
/// CustomerDataPacket được truyền qua <see cref="Initialize"/> từ CustomersPage
/// thay vì Shell query parameter, tránh việc phải serialize object qua URL.
/// </para>
/// </summary>
public partial class VehiclesPage : ContentPage
{
    private readonly VehiclesViewModel _vm;

    public VehiclesPage()
    {
        InitializeComponent();
        _vm = new VehiclesViewModel(
            new VehicleService(
                InstanceManager.Instance.GetOrCreateInstance<VehicleQueryCache>()));

        BindingContext = _vm;
    }

    /// <summary>
    /// Gọi từ CustomersPage trước khi navigate, truyền customer context.
    /// </summary>
    public void Initialize(CustomerDto owner) => _vm.Initialize(owner);

    /// <summary>Back button — navigate về CustomersPage.</summary>
    private async void OnBackClicked(System.Object sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }

        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }

    private async void OnFormBrandTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }

        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, "Hãng xe", vm.FormBrandOptions, idx => vm.FormPickerBrandIndex = idx);
    }

    private async void OnFormTypeTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }

        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, "Loại xe", vm.FormTypeOptions, idx => vm.FormPickerTypeIndex = idx);
    }

    private async void OnFormColorTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }

        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, "Màu sắc", vm.FormColorOptions, idx => vm.FormPickerColorIndex = idx);
    }

}
