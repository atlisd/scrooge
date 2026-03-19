namespace Cover.Web.Services;

public class CurrencyState
{
    public string Code { get; set; } = "ISK";
    public int Decimals { get; set; }

    public string Format(long minorUnits)
    {
        if (Decimals == 0)
            return $"{minorUnits:N0} {Code}";

        var value = minorUnits / (decimal)Math.Pow(10, Decimals);
        return $"{value.ToString($"N{Decimals}")} {Code}";
    }

    public long ToMinorUnits(decimal displayValue)
    {
        return (long)Math.Round(displayValue * (decimal)Math.Pow(10, Decimals));
    }

    public decimal ToDisplayValue(long minorUnits)
    {
        return minorUnits / (decimal)Math.Pow(10, Decimals);
    }
}
