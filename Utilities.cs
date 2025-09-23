
namespace Luxia;

public static class Utilities
{
    public static string FormatMoney(this double value, bool alwaysShowDecimals = true, bool alwaysShowOnlyForShort=true) => FormatMoney((long)value, alwaysShowDecimals, alwaysShowOnlyForShort);
    public static string FormatMoney(this float value, bool alwaysShowDecimals = true, bool alwaysShowOnlyForShort=true) => FormatMoney((long)value, alwaysShowDecimals, alwaysShowOnlyForShort);
    public static string FormatMoney(this int value, bool alwaysShowDecimals = true, bool alwaysShowOnlyForShort = true) => FormatMoney((long)value, alwaysShowDecimals, alwaysShowOnlyForShort);
    public static string FormatMoney(this long value, bool alwaysShowDecimals=true, bool alwaysShowOnlyForShort=true)
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

}
