using AutoX.Gara.Frontend.Controllers;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Services.Employees;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;
using System.Collections.Generic;
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
        => ShowFilterFlyout(sender, UiTextConfiguration.Current.EmployeesPickerFilterPositionText, vm => vm.FilterPositionOptions, (vm, idx) => vm.PickerPositionIndex = idx, vm => vm.PickPositionCommand);
    private void OnStatusFilterTapped(object? sender, TappedEventArgs e)
        => ShowFilterFlyout(sender, UiTextConfiguration.Current.EmployeesPickerFilterStatusText, vm => vm.FilterStatusOptions, (vm, idx) => vm.PickerStatusIndex = idx, vm => vm.PickStatusCommand);
    private void OnGenderFilterTapped(object? sender, TappedEventArgs e)
        => ShowFilterFlyout(sender, UiTextConfiguration.Current.EmployeesPickerFilterGenderText, vm => vm.FilterGenderOptions, (vm, idx) => vm.PickerGenderIndex = idx, vm => vm.PickGenderCommand);
    private void OnSalaryFilterTapped(object? sender, TappedEventArgs e)
        => ShowFilterFlyout(sender, UiTextConfiguration.Current.EmployeesPickerFilterSalaryText, vm => vm.SalaryFilterOptions, (vm, idx) => vm.PickerSalaryIndex = idx, vm => vm.PickSalaryCommand);
    private async void OnNewStatusTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not EmployeesViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, UiTextConfiguration.Current.EmployeesPickerChangeStatusText, vm.ChangeStatusOptions, idx => vm.NewStatusIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }
        string cancelText = UiTextConfiguration.Current.CommonActionCancelText;
        string pick = await page.DisplayActionSheetAsync(UiTextConfiguration.Current.EmployeesPickerChangeStatusText, cancelText, null, vm.ChangeStatusOptions);
        if (pick == cancelText || string.IsNullOrWhiteSpace(pick))
        {
            return;
        }
        int idx2 = Array.IndexOf(vm.ChangeStatusOptions, pick);
        if (idx2 >= 0)
        {
            vm.NewStatusIndex = idx2;
        }
    }
    private async void OnSalaryTypeTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not EmployeesViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, UiTextConfiguration.Current.EmployeesPickerSalaryTypeText, vm.SalaryFormTypeOptions, idx => vm.SalaryFormTypeIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }
        string cancelText = UiTextConfiguration.Current.CommonActionCancelText;
        string pick = await page.DisplayActionSheetAsync(UiTextConfiguration.Current.EmployeesPickerSalaryTypeText, cancelText, null, vm.SalaryFormTypeOptions);
        if (pick == cancelText || string.IsNullOrWhiteSpace(pick))
        {
            return;
        }
        int idx2 = Array.IndexOf(vm.SalaryFormTypeOptions, pick);
        if (idx2 >= 0)
        {
            vm.SalaryFormTypeIndex = idx2;
        }
    }
    private async void OnEmployeeFormGenderTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not EmployeesViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, UiTextConfiguration.Current.EmployeesPickerFormGenderText, vm.FormGenderOptions, idx => vm.FormGenderIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }
        string cancelText = UiTextConfiguration.Current.CommonActionCancelText;
        string pick = await page.DisplayActionSheetAsync(UiTextConfiguration.Current.EmployeesPickerFormGenderText, cancelText, null, vm.FormGenderOptions);
        if (pick == cancelText || string.IsNullOrWhiteSpace(pick))
        {
            return;
        }
        int idx2 = Array.IndexOf(vm.FormGenderOptions, pick);
        if (idx2 >= 0)
        {
            vm.FormGenderIndex = idx2;
        }
    }
    private async void OnEmployeeFormPositionTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not EmployeesViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, UiTextConfiguration.Current.EmployeesPickerFormPositionText, vm.FormPositionOptions, idx => vm.FormPositionIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }
        string cancelText = UiTextConfiguration.Current.CommonActionCancelText;
        string pick = await page.DisplayActionSheetAsync(UiTextConfiguration.Current.EmployeesPickerFormPositionText, cancelText, null, vm.FormPositionOptions);
        if (pick == cancelText || string.IsNullOrWhiteSpace(pick))
        {
            return;
        }
        int idx2 = Array.IndexOf(vm.FormPositionOptions, pick);
        if (idx2 >= 0)
        {
            vm.FormPositionIndex = idx2;
        }
    }
    private async void OnEmployeeFormStatusTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not EmployeesViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, UiTextConfiguration.Current.EmployeesPickerFormStatusText, vm.FormStatusOptions, idx => vm.FormStatusIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }
        string cancelText = UiTextConfiguration.Current.CommonActionCancelText;
        string pick = await page.DisplayActionSheetAsync(UiTextConfiguration.Current.EmployeesPickerFormStatusText, cancelText, null, vm.FormStatusOptions);
        if (pick == cancelText || string.IsNullOrWhiteSpace(pick))
        {
            return;
        }
        int idx2 = Array.IndexOf(vm.FormStatusOptions, pick);
        if (idx2 >= 0)
        {
            vm.FormStatusIndex = idx2;
        }
    }
    private void ShowFilterFlyout(
        object? sender,
        string title,
        Func<EmployeesViewModel, string[]> getOptions,
        Action<EmployeesViewModel, int> setIndex,
        Func<EmployeesViewModel, IAsyncRelayCommand> fallbackCommandGetter)
    {
        if (BindingContext is not EmployeesViewModel vm)
        {
            return;
        }
#if WINDOWS
        try
        {
            if (sender is not VisualElement anchor)
            {
                throw new InvalidOperationException("Missing anchor element for flyout.");
            }
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
#if WINDOWS
    private static bool TryShowFlyout(VisualElement? anchor, string title, IReadOnlyList<string> options, Action<int> onSelected)
    {
        try
        {
            if (anchor?.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement fe)
            {
                return false;
            }
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout
            {
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft
            };
            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = title, IsEnabled = false });
            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());
            for (int i = 0; i < options.Count; i++)
            {
                int idx = i;
                flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
                {
                    Text = options[i],
                    Command = new Command(() => onSelected(idx))
                });
            }
            flyout.ShowAt(fe);
            return true;
        }
        catch
        {
            return false;
        }
    }
#endif
}
