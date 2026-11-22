using System.Security.Cryptography;

namespace DigitalProductPass.Services;

public static class SerialCodeGenerator
{
    // Без O/0/I/1, за да няма объркване при ръчно въвеждане.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string New()
    {
        var chars = new char[8];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        var s = new string(chars);
        return $"DPP-{s[..4]}-{s[4..]}";
    }
}
