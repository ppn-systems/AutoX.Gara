// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Controllers;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Services.Vehicles;
using AutoX.Gara.Contracts.Protocol.Customers;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;
namespace AutoX.Gara.Frontend.Views;
/// <summary>
/// Code-behind c?a VehiclesPage.
/// <para>
/// CustomerDataPacket được truy?n qua <see cref="Initialize"/> t? CustomersPage
/// thay v� Shell query parameter, tr�nh vi?c ph?i serialize object qua URL.
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
    /// G?i t? CustomersPage tru?c khi navigate, truy?n customer context.
    /// </summary>
    public void Initialize(CustomerDto owner) => _vm.Initialize(owner);
    /// <summary>Back button � navigate v? CustomersPage.</summary>
    private async void OnBackClicked(object? sender, System.EventArgs e)
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
        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.VehiclesPickerFormBrandText, vm.FormBrandOptions, idx => vm.FormPickerBrandIndex = idx);
    }
    private async void OnFormTypeTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }
        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.VehiclesPickerFormTypeText, vm.FormTypeOptions, idx => vm.FormPickerTypeIndex = idx);
    }
    private async void OnFormColorTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }
        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.VehiclesPickerFormColorText, vm.FormColorOptions, idx => vm.FormPickerColorIndex = idx);
    }
}

