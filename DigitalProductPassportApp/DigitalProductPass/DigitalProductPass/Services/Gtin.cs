namespace DigitalProductPass.Services;

public static class Gtin
{
    /// <summary>Returns the GTIN left-padded to 14 digits, or null if it is not a valid GTIN-8/12/13/14.</summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var digits = new string(input.Where(char.IsAsciiDigit).ToArray());
        if (digits.Length is not (8 or 12 or 13 or 14)) return null;
        var padded = digits.PadLeft(14, '0');
        return HasValidCheckDigit(padded) ? padded : null;
    }

    public static bool HasValidCheckDigit(string gtin14)
    {
        if (gtin14.Length != 14 || !gtin14.All(char.IsAsciiDigit)) return false;
        int sum = 0;
        for (int i = 0; i < 13; i++)
            sum += (gtin14[i] - '0') * (i % 2 == 0 ? 3 : 1);
        return (10 - sum % 10) % 10 == gtin14[13] - '0';
    }
}
