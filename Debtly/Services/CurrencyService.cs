namespace Debtly.Services;

public static class CurrencyService
{
    public static string GetCurrencySymbol()
    {
        string currency =
            Preferences.Default.Get(
                "Currency",
                "₪ ILS");

        return currency switch
        {
            "$ USD" => "$",
            "€ EUR" => "€",
            "£ GBP" => "£",
            "₽ RUB" => "₽",
            _ => "₪"
        };
    }
}