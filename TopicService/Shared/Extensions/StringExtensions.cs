namespace Shared.Extensions;

public static class StringExtensions
{
    public static string ToSearchString(this string input) => $"%{input.Trim().ToLower()}%";
}
