// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Configuration;
using Nalix.Framework.Configuration.Binding;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AutoX.Gara.Frontend.Configuration;

public sealed class UiTextOptions : ConfigurationLoader
{
    public Dictionary<string, string> Entries { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public string LoginAppTitle { get; init; } = "AutoX Gara";
    public string LoginAppSubtitle { get; init; } = "Đăng nhập vào hệ thống";
    public string LoginUsernameLabel { get; init; } = "Tên đăng nhập";
    public string LoginUsernamePlaceholder { get; init; } = "Nhập tên đăng nhập";
    public string LoginPasswordLabel { get; init; } = "Mật khẩu";
    public string LoginPasswordPlaceholder { get; init; } = "Nhập mật khẩu";
    public string LoginButtonText { get; init; } = "Đăng nhập";
    public string RegisterButtonText { get; init; } = "Đăng ký tài khoản";
    public string LoginNetworkWarningText { get; init; } = "⚠ Chưa kết nối được server";
    public string LoginLoadingText { get; init; } = "Đang kết nối...";
    public string PopupOkButtonText { get; init; } = "OK";
    public string PopupRetryButtonText { get; init; } = "Thử lại";
    public string LoginFailedTitleText { get; init; } = "Đăng nhập thất bại";
    public string RegisterFailedTitleText { get; init; } = "Đăng ký thất bại";
    public string RegisterSuccessTitleText { get; init; } = "Đăng ký thành công";
    public string RegisterSuccessMessageText { get; init; } = "Tài khoản đã được tạo. Bạn có thể đăng nhập ngay.";
    public string UsernameRequiredErrorText { get; init; } = "Tên đăng nhập không được để trống.";
    public string PasswordRequiredErrorText { get; init; } = "Mật khẩu không được để trống.";
    public string UsernameInvalidErrorText { get; init; } = "Tên đăng nhập không hợp lệ (5-50 ký tự).";
    public string PasswordInvalidErrorText { get; init; } = "Mật khẩu không đủ mạnh (tối thiểu 8 ký tự, bao gồm chữ hoa/thường/số/ký tự đặc biệt).";
    public string AuthRejectedMessageText { get; init; } = "Máy chủ từ chối yêu cầu.";
    public string RetryLaterMessageText { get; init; } = "Vui lòng thử lại sau.";
    public string ServerErrorTitleText { get; init; } = "Lỗi máy chủ";
    public string ConnectFailedMessageText { get; init; } = "Không thể kết nối tới hạ tầng Nalix.";

    public string Get(string key, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(key)
            && Entries.TryGetValue(key, out string? value)
            && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    public string Format(string key, string fallbackFormat, params object[] args)
    {
        string format = Get(key, fallbackFormat);
        return string.Format(CultureInfo.CurrentCulture, format, args);
    }
}

public static class UiTextConfiguration
{
    public static UiTextOptions Current => ConfigurationManager.Instance.Get<UiTextOptions>();
}

public static class UiText
{
    public static string Get(string key, string fallback) => UiTextConfiguration.Current.Get(key, fallback);

    public static string Format(string key, string fallbackFormat, params object[] args) =>
        UiTextConfiguration.Current.Format(key, fallbackFormat, args);
}
