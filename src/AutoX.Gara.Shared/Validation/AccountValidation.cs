// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Validation;

/// <summary>
/// Validation rules cho credentials — giống server để early-reject trước khi gửi packet.
/// Nếu server thay đổi rules, chỉ cần sửa ở đây + AccountOps.
/// </summary>
public static class AccountValidation
{
    /// <summary>
    /// Validate username: không rỗng, ≤50 ký tự, chỉ a-z A-Z 0-9 _ -
    /// </summary>
    public static System.Boolean IsValidUsername(System.String username)
    {
        if (System.String.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        if (username.Length > 50)
        {
            return false;
        }

        foreach (System.Char c in username)
        {
            if (!IsAllowedUsernameChar(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validate password: ≥8 ký tự, có hoa, thường, số, ký tự đặc biệt.
    /// </summary>
    public static System.Boolean IsValidPassword(System.String password)
        => !System.String.IsNullOrWhiteSpace(password)
        && password.Length >= 8
        && System.Linq.Enumerable.Any(password, System.Char.IsLower)
        && System.Linq.Enumerable.Any(password, System.Char.IsUpper)
        && System.Linq.Enumerable.Any(password, System.Char.IsDigit)
        && !System.Linq.Enumerable.All(password, System.Char.IsLetterOrDigit);

    /// <summary>
    /// Validates an email address using a lightweight algorithm.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the email appears structurally valid; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This validation performs basic structural checks:
    /// <list type="bullet">
    /// <item>Must contain exactly one '@'.</item>
    /// <item>Must contain a '.' after '@'.</item>
    /// <item>No spaces or consecutive dots.</item>
    /// <item>Local part, domain, and TLD must be non-empty.</item>
    /// </list>
    /// It does not guarantee that the domain exists.
    /// </remarks>
    public static System.Boolean IsValidEmail(System.String email)
    {
        if (System.String.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        if (email.Contains(' '))
        {
            return false;
        }

        System.Int32 atIndex = email.IndexOf('@');
        System.Int32 dotIndex = email.LastIndexOf('.');

        if (atIndex <= 0)
        {
            return false;
        }

        if (dotIndex < atIndex + 2)
        {
            return false;
        }

        if (dotIndex == email.Length - 1)
        {
            return false;
        }

        if (email.IndexOf('@', atIndex + 1) != -1)
        {
            return false;
        }

        for (System.Int32 i = 1; i < email.Length; i++)
        {
            if (email[i] == '.' && email[i - 1] == '.')
            {
                return false;
            }
        }

        System.String local = email[..atIndex];
        System.String domain = email.Substring(atIndex + 1, dotIndex - atIndex - 1);
        System.String tld = email[(dotIndex + 1)..];

        return !System.String.IsNullOrWhiteSpace(local)
            && !System.String.IsNullOrWhiteSpace(domain)
            && !System.String.IsNullOrWhiteSpace(tld);
    }

    /// <summary>
    /// Validates a Vietnamese phone number using a lightweight algorithm.
    /// </summary>
    /// <param name="phone">The phone number to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the phone number appears structurally valid; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Rules applied:
    /// <list type="bullet">
    /// <item>Only numeric digits allowed.</item>
    /// <item>Length must be 10 or 11 characters.</item>
    /// <item>Must start with '0'.</item>
    /// <item>Valid prefixes: 03, 05, 07, 08, 09.</item>
    /// </list>
    /// </remarks>
    public static System.Boolean IsValidVietnamPhoneNumber(System.String phone)
    {
        if (System.String.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        if (phone.Contains(' '))
        {
            return false;
        }

        foreach (System.Char c in phone)
        {
            if (c is < '0' or > '9')
            {
                return false;
            }
        }

        if (phone.Length is not 10 and not 11)
        {
            return false;
        }

        if (phone[0] != '0')
        {
            return false;
        }

        System.Boolean prefixOk = false;
        System.String[] validPrefixes = ["03", "05", "07", "08", "09"];

        foreach (System.String prefix in validPrefixes)
        {
            if (phone.StartsWith(prefix))
            {
                prefixOk = true;
                break;
            }
        }

        return prefixOk;
    }

    private static System.Boolean IsAllowedUsernameChar(System.Char c)
        => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_' or '-';
}