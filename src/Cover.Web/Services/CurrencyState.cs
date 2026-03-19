using System.Globalization;

namespace Cover.Web.Services;

public class CurrencyState
{
    private record CurrencyFormat(string CultureName, string GroupSep, string DecimalSep, Func<DateOnly, string> FormatDate);

    private static readonly Dictionary<string, CurrencyFormat> _formats = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ISK"] = new("is-IS", ".", ",", d => $"{d.Day}.{d.Month}.{d.Year}"),
        ["DKK"] = new("da-DK", ".", ",", d => $"{d.Day}.{d.Month}.{d.Year}"),
        ["NOK"] = new("nb-NO", "\u00a0", ",", d => $"{d.Day}.{d.Month}.{d.Year}"),
        ["SEK"] = new("sv-SE", "\u00a0", ",", d => $"{d.Year}-{d.Month:D2}-{d.Day:D2}"),
        ["EUR"] = new("de-DE", ".", ",", d => $"{d.Day}.{d.Month}.{d.Year}"),
        ["GBP"] = new("en-GB", ",", ".", d => $"{d.Day:D2}/{d.Month:D2}/{d.Year}"),
        ["USD"] = new("en-US", ",", ".", d => $"{d.Month}/{d.Day}/{d.Year}"),
        ["CAD"] = new("en-CA", ",", ".", d => $"{d.Month}/{d.Day}/{d.Year}"),
        ["AUD"] = new("en-AU", ",", ".", d => $"{d.Day}/{d.Month:D2}/{d.Year}"),
        ["CHF"] = new("de-CH", "'", ".", d => $"{d.Day}.{d.Month}.{d.Year}"),
        ["JPY"] = new("ja-JP", ",", ".", d => $"{d.Year}/{d.Month}/{d.Day}"),
    };

    private NumberFormatInfo _numberFormat = NumberFormatInfo.InvariantInfo;
    private Func<DateOnly, string> _formatDate = d => $"{d.Day}.{d.Month}.{d.Year}";

    public string Code { get; private set; } = "ISK";
    public int Decimals { get; private set; }
    public CultureInfo? Culture { get; private set; }

    public void SetCurrency(string code, int decimals)
    {
        Code = code;
        Decimals = decimals;

        if (_formats.TryGetValue(code, out var fmt))
        {
            var nf = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
            nf.NumberGroupSeparator = fmt.GroupSep;
            nf.NumberDecimalSeparator = fmt.DecimalSep;
            nf.NumberGroupSizes = new[] { 3 };
            _numberFormat = nf;
            _formatDate = fmt.FormatDate;

            try { Culture = new CultureInfo(fmt.CultureName); }
            catch { /* culture not available in WASM */ }
        }
    }

    public string Format(long minorUnits)
    {
        if (Decimals == 0)
            return minorUnits.ToString("N0", _numberFormat) + " " + Code;

        var value = minorUnits / (decimal)Math.Pow(10, Decimals);
        return value.ToString($"N{Decimals}", _numberFormat) + " " + Code;
    }

    public string FormatDate(DateOnly date) => _formatDate(date);

    public long ToMinorUnits(decimal displayValue) =>
        (long)Math.Round(displayValue * (decimal)Math.Pow(10, Decimals));

    public decimal ToDisplayValue(long minorUnits) =>
        minorUnits / (decimal)Math.Pow(10, Decimals);
}
