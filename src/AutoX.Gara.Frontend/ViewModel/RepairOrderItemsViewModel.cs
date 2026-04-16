// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.Services.Repairs;
using AutoX.Gara.Shared.Protocol.Inventory;
using AutoX.Gara.Shared.Protocol.Invoices;
using AutoX.Gara.Shared.Protocol.Repairs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Nalix.Framework.Injection;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoX.Gara.Frontend.Controllers.Billings;

public sealed partial class RepairOrderItemsViewModel : ObservableObject, System.IDisposable
{
    private readonly RepairOrderItemService _service;
    private System.Threading.CancellationTokenSource? _cts;
    private readonly PartService _partService;
    private System.Threading.CancellationTokenSource? _lookupCts;

    private readonly System.Collections.Generic.Dictionary<System.Int32, System.String> _partNameById = [];
    private System.Collections.Generic.List<RepairOrderItemDto> _rawItems = [];

    private const System.Int32 DefaultPageSize = 20;

    public RepairOrderItemsViewModel(RepairOrderItemService service, PartService partService)
    {
        _service = service ?? throw new System.ArgumentNullException(nameof(service));
        _partService = partService ?? throw new System.ArgumentNullException(nameof(partService));
    }

    public sealed record LookupOption(System.Int32 Id, System.String Display);

    public sealed class RepairOrderItemRow
    {
        public required RepairOrderItemDto Item { get; init; }
        public System.String PartName { get; init; } = System.String.Empty;
    }

    [ObservableProperty] public partial RepairOrderDto? RepairOrder { get; set; }

    public System.String PageTitle => RepairOrder?.RepairOrderId is null
        ? "Part trong lệnh"
        : $"Part lệnh #{RepairOrder.RepairOrderId}";

    public ObservableCollection<RepairOrderItemRow> ItemRows { get; } = [];

    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial bool HasNextPage { get; set; }
    [ObservableProperty] public partial bool HasPreviousPage { get; set; }
    [ObservableProperty] public partial int TotalCount { get; set; }
    public System.Int32 TotalPages => TotalCount > 0
        ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize)
        : 0;

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial bool IsLookupLoading { get; set; }
    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    // Lookups
    public ObservableCollection<LookupOption> PartOptions { get; } = [];
    [ObservableProperty] public partial LookupOption? SelectedPartOption { get; set; }

    // Filterable selector overlay
    [ObservableProperty] public partial bool IsPartSelectorVisible { get; set; }
    public ObservableCollection<LookupOption> FilteredPartOptions { get; } = [];
    [ObservableProperty] public partial string PartSearchTerm { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial bool IsEditing { get; set; }
    [ObservableProperty] public partial RepairOrderItemDto? SelectedItem { get; set; }

    [ObservableProperty] public partial int FormPartId { get; set; }
    [ObservableProperty] public partial int FormQuantity { get; set; } = 1;

    [ObservableProperty] public partial bool IsDeleteConfirmVisible { get; set; }

    public void Initialize(RepairOrderDto repairOrder)
    {
        RepairOrder = repairOrder;
        OnPropertyChanged(nameof(PageTitle));
        _ = LoadLookupsAsync();
        _ = LoadAsync();
    }

    partial void OnSelectedPartOptionChanged(LookupOption? value)
        => FormPartId = value?.Id ?? 0;

    private void SyncSelectedPartFromId()
    {
        if (FormPartId > 0 && (SelectedPartOption?.Id ?? 0) != FormPartId)
        {
            SelectedPartOption = PartOptions.FirstOrDefault(x => x.Id == FormPartId);
        }
    }

    private static System.Boolean ContainsIgnoreCase(System.String haystack, System.String needle)
        => haystack?.IndexOf(needle ?? System.String.Empty, System.StringComparison.OrdinalIgnoreCase) >= 0;

    private void RefreshPartFilter()
    {
        FilteredPartOptions.Clear();
        System.String term = PartSearchTerm?.Trim() ?? System.String.Empty;
        for (System.Int32 i = 0; i < PartOptions.Count; i++)
        {
            var opt = PartOptions[i];
            if (term.Length == 0 || ContainsIgnoreCase(opt.Display, term))
            {
                FilteredPartOptions.Add(opt);
            }
        }
    }

    partial void OnPartSearchTermChanged(string value) => RefreshPartFilter();

    [RelayCommand]
    private void OpenPartSelector()
    {
        PartSearchTerm = System.String.Empty;
        RefreshPartFilter();
        IsPartSelectorVisible = true;

        // If user opens selector before lookups finished, ensure we start loading.
        if (PartOptions.Count == 0 && !IsLookupLoading)
        {
            _ = LoadLookupsAsync();
        }
    }

    [RelayCommand]
    private void ClosePartSelector() => IsPartSelectorVisible = false;

    [RelayCommand]
    private void SelectPart(LookupOption opt)
    {
        SelectedPartOption = opt;
        IsPartSelectorVisible = false;
    }

    private void RefreshRowDisplays()
    {
        ItemRows.Clear();
        for (System.Int32 i = 0; i < _rawItems.Count; i++)
        {
            RepairOrderItemDto it = _rawItems[i];
            _partNameById.TryGetValue(it.PartId, out System.String? name);
            ItemRows.Add(new RepairOrderItemRow
            {
                Item = it,
                PartName = name ?? $"#{it.PartId}"
            });
        }
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task LoadLookupsAsync()
    {
        _lookupCts?.Cancel();
        _lookupCts?.Dispose();
        _lookupCts = new System.Threading.CancellationTokenSource();
        var ct = _lookupCts.Token;

        try
        {
            IsLookupLoading = true;
            var result = await _partService.GetListAsync(
                page: 1,
                pageSize: 200,
                ct: ct);

            if (!result.IsSuccess)
            {
                HasError = true;
                ErrorMessage = $"Không tải được danh sách part: {result.ErrorMessage ?? "Thao tác thất bại."}";
                return;
            }

            PartOptions.Clear();
            _partNameById.Clear();
            for (System.Int32 i = 0; i < result.Parts.Count; i++)
            {
                PartDto p = result.Parts[i];
                if (p.PartId.HasValue && p.PartId.Value > 0)
                {
                    System.String display = $"#{p.PartId.Value} - {p.PartName} ({p.SellingPrice:n0})";
                    PartOptions.Add(new LookupOption(
                        p.PartId.Value,
                        display));
                    _partNameById[p.PartId.Value] = display;
                }
            }

            SyncSelectedPartFromId();
            RefreshRowDisplays();
            RefreshPartFilter();
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            HasError = true;
            ErrorMessage = $"Không tải được danh sách part: {ex.Message}";
        }
        finally
        {
            IsLookupLoading = false;
        }
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task LoadAsync()
    {
        if (RepairOrder?.RepairOrderId is null)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsLoading = true;
        ClearError();

        try
        {
            RepairOrderItemListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                filterRepairOrderId: RepairOrder.RepairOrderId.Value,
                ct: ct);

            if (!result.IsSuccess)
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Thao tác thất bại.";
                return;
            }

            _rawItems = result.RepairOrderItems ?? [];
            RefreshRowDisplays();

            TotalCount = result.TotalCount;
            HasPreviousPage = CurrentPage > 1;
            HasNextPage = CurrentPage < TotalPages;
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnCurrentPageChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    private void NextPage()
    {
        if (HasNextPage)
        {
            CurrentPage++;
        }
    }
    [RelayCommand]
    private void PreviousPage()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
        }
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        ClearError();
        IsEditing = false;
        SelectedItem = null;
        FormPartId = 0;
        SelectedPartOption = null;
        FormQuantity = 1;
        IsFormVisible = true;

        // Preload part list so user can select immediately.
        if (PartOptions.Count == 0 && !IsLookupLoading)
        {
            _ = LoadLookupsAsync();
        }
    }

    [RelayCommand]
    private void OpenEditForm(RepairOrderItemDto item)
    {
        IsEditing = true;
        SelectedItem = item;
        FormPartId = item.PartId;
        SyncSelectedPartFromId();
        FormQuantity = item.Quantity;
        IsFormVisible = true;
    }

    [RelayCommand] private void CancelForm() => IsFormVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveAsync()
    {
        if (RepairOrder?.RepairOrderId is null)
        {
            return;
        }

        if (SelectedPartOption is null || FormPartId <= 0 || FormQuantity <= 0)
        {
            HasError = true;
            ErrorMessage = "Vui lòng chọn Part và nhập Quantity > 0.";
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            RepairOrderItemDto packet = new()
            {
                RepairOrderItemId = IsEditing ? SelectedItem?.RepairOrderItemId : null,
                RepairOrderId = RepairOrder.RepairOrderId.Value,
                PartId = FormPartId,
                Quantity = FormQuantity
            };

            RepairOrderItemWriteResult result = IsEditing
                ? await _service.UpdateAsync(packet)
                : await _service.CreateAsync(packet);

            if (!result.IsSuccess)
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Thao tác thất bại.";
                return;
            }

            ClearError();
            IsFormVisible = false;
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void RequestDelete(RepairOrderItemDto item)
    {
        SelectedItem = item;
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand] private void CancelDelete() => IsDeleteConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeleteAsync()
    {
        if (SelectedItem?.RepairOrderItemId is null)
        {
            return;
        }

        IsDeleteConfirmVisible = false;
        IsLoading = true;
        ClearError();

        try
        {
            RepairOrderItemWriteResult result = await _service.DeleteAsync(SelectedItem);
            if (!result.IsSuccess)
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Thao tác thất bại.";
                return;
            }
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        var cts = System.Threading.Interlocked.Exchange(ref _cts, null);
        var lookupCts = System.Threading.Interlocked.Exchange(ref _lookupCts, null);

        if (lookupCts is not null)
        {
            try
            {
                lookupCts.Cancel();
            }
            catch (System.ObjectDisposedException)
            {
            }
            lookupCts.Dispose();
        }

        if (cts is null)
        {
            return;
        }

        try
        {
            cts.Cancel();
        }
        catch (System.ObjectDisposedException)
        {
        }

        cts.Dispose();
    }

    private static void LogException(System.Exception ex)
    {
        // Keep logging consistent with global crash logging setup.
        try
        {
            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
            logger?.Error(ex.ToString());
            if (ex.InnerException is not null)
            {
                logger?.Error("Inner: " + ex.InnerException);
            }
        }
        catch
        {
            // Swallow: logging should never crash UI.
        }
    }

    private void ClearError() { HasError = false; ErrorMessage = null; }
}
