// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend;

public partial class CustomersPage : ContentPage
{
    public CustomersPage()
    {
        InitializeComponent();
        BindingContext = new CustomersViewModel(new CustomerService());
    }
}