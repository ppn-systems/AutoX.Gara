// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Employees;
using AutoX.Gara.Frontend.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Page for employee management.
/// </summary>
public partial class EmployeesPage : ContentPage
{
    /// <summary>
    /// Initializes the EmployeesPage and sets up the ViewModel.
    /// </summary>
    public EmployeesPage()
    {
        InitializeComponent();
        BindingContext = new EmployeesViewModel(
            new EmployeeService(
                InstanceManager.Instance.GetOrCreateInstance<EmployeeQueryCache>()),
            new EmployeeSalaryService(
                InstanceManager.Instance.GetOrCreateInstance<EmployeeSalaryQueryCache>()));
    }

    /// <summary>
    /// Loads data when page appears if not already loaded.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EmployeesViewModel vm && vm.Employees.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }

    private void OnPositionFilterTapped(object? sender, TappedEventArgs e)
        => ShowFilterFlyout(sender, "Chọn chức vụ", vm => vm.FilterPositionOptions, (vm, idx) => vm.PickerPositionIndex = idx, vm => vm.PickPositionCommand);

    private void OnStatusFilterTapped(object? sender, TappedEventArgs e)
        => ShowFilterFlyout(sender, "Chọn trạng thái", vm => vm.FilterStatusOptions, (vm, idx) => vm.PickerStatusIndex = idx, vm => vm.PickStatusCommand);

    private void OnGenderFilterTapped(object? sender, TappedEventArgs e)
        => ShowFilterFlyout(sender, "Chọn giới tính", vm => vm.FilterGenderOptions, (vm, idx) => vm.PickerGenderIndex = idx, vm => vm.PickGenderCommand);

    private void OnSalaryFilterTapped(object? sender, TappedEventArgs e)
        => ShowFilterFlyout(sender, "Lọc theo lương", vm => vm.SalaryFilterOptions, (vm, idx) => vm.PickerSalaryIndex = idx, vm => vm.PickSalaryCommand);

    private void ShowFilterFlyout(
        object? sender,
        string title,
        Func<EmployeesViewModel, string[]> getOptions,
        Action<EmployeesViewModel, int> setIndex,
        Func<EmployeesViewModel, IAsyncRelayCommand> fallbackCommandGetter)
    {
        if (BindingContext is not EmployeesViewModel vm)
            return;

#if WINDOWS
        try
        {
            if (sender is not VisualElement anchor)
                throw new InvalidOperationException("Missing anchor element for flyout.");

            var options = getOptions(vm);

            // Build a WinUI MenuFlyout and show it anchored under the clicked filter "button".
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();

            // Optional title-like first item (disabled) to make it clearer.
            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = title,
                IsEnabled = false
            });
            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            for (int i = 0; i < options.Length; i++)
            {
                int idx = i;
                flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
                {
                    Text = options[i],
                    Command = new Command(() => setIndex(vm, idx))
                });
            }

            flyout.Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft;

            if (anchor.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement fe)
            {
                flyout.ShowAt(fe);
                return;
            }
        }
        catch
        {
            // Ignore and fall back to ActionSheet below.
        }
#endif

        // Non-Windows platforms (or when WinUI anchor fails): fall back to ActionSheet.
        var cmd = fallbackCommandGetter(vm);
        if (cmd.CanExecute(null))
        {
            _ = cmd.ExecuteAsync(null);
        }
    }
}
