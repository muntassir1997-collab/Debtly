using Debtly.Services;

namespace Debtly;

public partial class MainPage : ContentPage
{
    public double[] BalanceChartValues { get; private set; } =
        Array.Empty<double>();

    public string[] BalanceChartLabels { get; private set; } =
        Array.Empty<string>();

    public MainPage()
    {
        InitializeComponent();

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadBalanceAsync();
    }

    private async Task LoadBalanceAsync()
    {
        var transactions =
            await App.Database.GetAllTransactionsAsync();

        decimal owedToYou = 0;
        decimal youOwe = 0;

        foreach (var transaction in transactions)
        {
            if (transaction.IsFromPerson)
            {
                youOwe += transaction.Amount;
            }
            else
            {
                owedToYou += transaction.Amount;
            }
        }

        decimal netBalance =
            owedToYou - youOwe;

        string currency =
            CurrencyService.GetCurrencySymbol();

        YouOweLabel.Text =
            $"{currency} {youOwe:0.00}";

        OwedToYouLabel.Text =
            $"{currency} {owedToYou:0.00}";

        NetBalanceLabel.Text =
            netBalance > 0
                ? $"+ {currency} {netBalance:0.00}"
                : netBalance < 0
                    ? $"- {currency} {Math.Abs(netBalance):0.00}"
                    : $"{currency} 0.00";

        UpdateNetBalanceColor(netBalance);

        LoadBalanceChart(transactions);
    }

    private void UpdateNetBalanceColor(
        decimal netBalance)
    {
        if (netBalance > 0)
        {
            NetBalanceLabel.TextColor =
                Color.FromArgb("#34C759");
        }
        else if (netBalance < 0)
        {
            NetBalanceLabel.TextColor =
                Color.FromArgb("#FF453A");
        }
        else
        {
            bool isDark =
                Application.Current?.RequestedTheme ==
                AppTheme.Dark;

            NetBalanceLabel.TextColor =
                isDark
                    ? Colors.White
                    : Color.FromArgb("#1C1C1E");
        }
    }

    private void LoadBalanceChart(
        List<Models.Transaction> transactions)
    {
        decimal balance = 0;

        var orderedTransactions =
            transactions
                .OrderBy(t => t.Date)
                .ToList();

        var values = new List<double>();
        var labels = new List<string>();

        int total =
            orderedTransactions.Count;

        for (int i = 0; i < total; i++)
        {
            var transaction =
                orderedTransactions[i];

            if (transaction.IsFromPerson)
            {
                balance -= transaction.Amount;
            }
            else
            {
                balance += transaction.Amount;
            }

            values.Add((double)balance);

            labels.Add(
                GetChartLabel(
                    orderedTransactions,
                    i));
        }

        BalanceChartValues =
            values.ToArray();

        BalanceChartLabels =
            labels.ToArray();

        OnPropertyChanged(
            nameof(BalanceChartValues));

        OnPropertyChanged(
            nameof(BalanceChartLabels));
    }

    private string GetChartLabel(
        List<Models.Transaction> transactions,
        int index)
    {
        int total =
            transactions.Count;

        if (total <= 6)
        {
            return transactions[index]
                .Date
                .ToString("dd MMM");
        }

        int labelCount = 6;

        int step =
            (int)Math.Ceiling(
                (double)(total - 1) /
                (labelCount - 1));

        if (index % step == 0 ||
            index == total - 1)
        {
            return transactions[index]
                .Date
                .ToString("dd MMM");
        }

        return string.Empty;
    }
}