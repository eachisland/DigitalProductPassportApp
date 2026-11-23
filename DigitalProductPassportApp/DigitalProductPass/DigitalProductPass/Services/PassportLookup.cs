using System.Linq.Expressions;
using DigitalProductPass.Models;

namespace DigitalProductPass.Services;

internal static class PassportLookup
{
    public static Expression<Func<ProductItem, bool>> MatchGs1(string gtin, string serial, string? batch = null)
    {
        var serialCode = serial.ToUpperInvariant();
        return item => item.SerialCode == serialCode && item.Product.Gtin == gtin
            && (batch == null || item.BatchNumber == batch);
    }
}
