using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DigitalProductPass.Services;

/// <summary>
/// Builds unique variant serial SKUs: {DIFFERENTIATION}-{COLOR}-{SIZE}-{RANDOM}
/// e.g. TSHORG-WHI-M-7K3M9. Max 20 characters so it is also a valid GS1 AI (21) serial number.
/// Uniqueness against the database is enforced by the caller (unique index on ProductItems.SerialCode).
/// </summary>
public static class SerialCodeGenerator
{
    // No O/0/I/1 in the random part to avoid confusion when typed by hand.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public const int DifferentiationLength = 6;
    public const int ColorLength = 3;
    public const int SizeLength = 3;
    public const int SuffixLength = 5;

    public static string New(string? differentiationCode, string? color, string? size)
    {
        var diff = Clean(differentiationCode, DifferentiationLength, "GEN");
        var col = Clean(color, ColorLength, "XXX");
        var sz = Clean(size, SizeLength, "OS");
        return $"{diff}-{col}-{sz}-{Suffix(SuffixLength)}";
    }

    private static string Suffix(int length)
    {
        var chars = new char[length];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(chars);
    }

    /// <summary>Upper-cases, strips diacritics (ş→S, ü→U, ß→SS…) and keeps only A-Z / 0-9.</summary>
    private static string Clean(string? input, int max, string fallback)
    {
        if (string.IsNullOrWhiteSpace(input)) return fallback;

        var normalized = input.Trim()
            .Replace("ß", "ss")
            .Replace("ı", "i")
            .Replace("İ", "I")
            .Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(max);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            {
                sb.Append(char.ToUpperInvariant(ch));
                if (sb.Length == max) break;
            }
        }
        return sb.Length == 0 ? fallback : sb.ToString();
    }
}
