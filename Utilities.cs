
using System.Globalization;

namespace Luxia;

public static class Utilities
{
    public static string Shorten(this double value, bool alwaysShowDecimals = true, bool alwaysShowOnlyForShort=true) => Shorten((long)value, alwaysShowDecimals, alwaysShowOnlyForShort);
    public static string Shorten(this float value, bool alwaysShowDecimals = true, bool alwaysShowOnlyForShort=true) => Shorten((long)value, alwaysShowDecimals, alwaysShowOnlyForShort);
    public static string Shorten(this int value, bool alwaysShowDecimals = true, bool alwaysShowOnlyForShort = true) => Shorten((long)value, alwaysShowDecimals, alwaysShowOnlyForShort);
    public static string Shorten(this long value, bool alwaysShowDecimals=true, bool alwaysShowOnlyForShort=true)
    {
        if (value < 1_000)
            return value.ToString(alwaysShowDecimals && !alwaysShowOnlyForShort ? "0.00" : "0.##");

        if (value < 1_000_000)
            return (value / 1_000D).ToString(alwaysShowDecimals ? "0.00k" : "0.##k");

        if (value < 1_000_000_000)
            return (value / 1_000_000D).ToString(alwaysShowDecimals ? "0.00m" : "0.##m");

        if (value < 1_000_000_000_000)
            return (value / 1_000_000_000D).ToString(alwaysShowDecimals ? "0.00b" : "0.##b");

        return (value / 1_000_000_000_000D).ToString(alwaysShowDecimals ? "0.00t" : "0.##t");
    }

    public static string Beautify(this int value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    public static string Beautify(this long value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    public static string Beautify(this float value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    public static string Beautify(this double value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);
}