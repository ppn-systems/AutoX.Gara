// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Validator;

/// <summary>
/// Validation rules cho credentials — giống server để early-reject trước khi gửi packet.
/// Nếu server thay đổi rules, chỉ cần sửa ở đây + AccountOps.
/// </summary>
public static class CredentialValidator
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

    private static System.Boolean IsAllowedUsernameChar(System.Char c)
        => c is (>= 'a' and <= 'z')
            or (>= 'A' and <= 'Z')
            or (>= '0' and <= '9')
            or '_' or '-';
}