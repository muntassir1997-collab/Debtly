using Debtly.Models;
using Debtly.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Debtly;

public partial class PersonPage : ContentPage
{
    private readonly Person _person;

    public PersonPage(Person person)
    {
        InitializeComponent();

        _person = person;

        PersonNameLabel.Text =
            person.Name;

        Shell.SetTabBarIsVisible(
            this,
            false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadTransactionsAsync();
    }

    private async Task LoadTransactionsAsync()
    {
        var transactions =
            await App.Database.GetTransactionsForPersonAsync(
                _person.Id);

        TransactionsList.Children.Clear();

        string currency =
            CurrencyService.GetCurrencySymbol();

        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;

        Color primaryText =
            isDark
                ? Color.FromArgb("#FFFFFF")
                : Color.FromArgb("#1C1C1E");

        Color secondaryText =
            isDark
                ? Color.FromArgb("#8C9BA8")
                : Color.FromArgb("#8E8E93");

        Color cardBackground =
            isDark
                ? Color.FromArgb("#202B36")
                : Colors.White;

        Color editBackground =
            isDark
                ? Color.FromArgb("#1E3A5F")
                : Color.FromArgb("#EAF3FF");

        Color deleteBackground =
            isDark
                ? Color.FromArgb("#4A2928")
                : Color.FromArgb("#FFF0EF");

        if (transactions.Count == 0)
        {
            TransactionsList.Children.Add(
                new Label
                {
                    Text =
                        "No transactions yet",

                    FontSize = 16,

                    TextColor =
                        secondaryText,

                    HorizontalOptions =
                        LayoutOptions.Center
                });

            BalanceTitleLabel.Text =
                "Settled";

            BalanceLabel.Text =
                $"{currency} 0.00";

            BalanceLabel.TextColor =
                secondaryText;

            return;
        }

        decimal balance = 0;

        foreach (var transaction in transactions)
        {
            if (transaction.IsFromPerson)
            {
                balance -=
                    transaction.Amount;
            }
            else
            {
                balance +=
                    transaction.Amount;
            }

            string amountText =
                transaction.IsFromPerson
                    ? $"- {currency} {transaction.Amount:0.00}"
                    : $"+ {currency} {transaction.Amount:0.00}";

            Color amountColor =
                transaction.IsFromPerson
                    ? Color.FromArgb("#FF453A")
                    : Color.FromArgb("#34C759");

            string description =
                transaction.IsFromPerson
                    ? "They gave"
                    : "You gave";

            string dateText =
                transaction.Date.ToString(
                    "dd MMM yyyy");

            var transactionInfo =
                new VerticalStackLayout
                {
                    Spacing = 4
                };

            transactionInfo.Children.Add(
                new Label
                {
                    Text =
                        amountText,

                    FontSize = 20,

                    FontAttributes =
                        FontAttributes.Bold,

                    TextColor =
                        amountColor
                });

            transactionInfo.Children.Add(
                new Label
                {
                    Text =
                        description,

                    FontSize = 14,

                    TextColor =
                        secondaryText
                });

            if (!string.IsNullOrWhiteSpace(
                    transaction.Note))
            {
                transactionInfo.Children.Add(
                    new Label
                    {
                        Text =
                            transaction.Note,

                        FontSize = 14,

                        TextColor =
                            secondaryText,

                        LineBreakMode =
                            LineBreakMode.TailTruncation
                    });
            }

            transactionInfo.Children.Add(
                new Label
                {
                    Text =
                        dateText,

                    FontSize = 12,

                    TextColor =
                        secondaryText
                });

            var editButton =
                new Button
                {
                    Text =
                        "Edit",

                    FontSize = 13,

                    FontAttributes =
                        FontAttributes.None,

                    HeightRequest = 34,

                    MinimumWidthRequest = 58,

                    Padding =
                        new Thickness(10, 0),

                    CornerRadius = 10,

                    BackgroundColor =
                        editBackground,

                    TextColor =
                        Color.FromArgb("#007AFF"),

                    HorizontalOptions =
                        LayoutOptions.End,

                    VerticalOptions =
                        LayoutOptions.Center
                };

            editButton.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(
                    new EditTransactionPage(
                        transaction));
            };

            var deleteButton =
                new Button
                {
                    Text =
                        "Delete",

                    FontSize = 13,

                    FontAttributes =
                        FontAttributes.None,

                    HeightRequest = 34,

                    MinimumWidthRequest = 62,

                    Padding =
                        new Thickness(10, 0),

                    CornerRadius = 10,

                    BackgroundColor =
                        deleteBackground,

                    TextColor =
                        Color.FromArgb("#FF3B30"),

                    HorizontalOptions =
                        LayoutOptions.End,

                    VerticalOptions =
                        LayoutOptions.Center
                };

            deleteButton.Clicked += async (sender, e) =>
            {
                bool confirm =
                    await DisplayAlert(
                        "Delete Transaction",
                        "Are you sure you want to delete this transaction?",
                        "Delete",
                        "Cancel");

                if (!confirm)
                    return;

                await App.Database.DeleteTransactionAsync(
                    transaction.Id);

                await LoadTransactionsAsync();
            };

            var buttons =
                new HorizontalStackLayout
                {
                    Spacing = 6,

                    HorizontalOptions =
                        LayoutOptions.End,

                    VerticalOptions =
                        LayoutOptions.Center,

                    Children =
                    {
                        editButton,
                        deleteButton
                    }
                };

            var cardContent =
                new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(
                            GridLength.Star),

                        new ColumnDefinition(
                            GridLength.Auto)
                    },

                    ColumnSpacing = 12,

                    Padding =
                        new Thickness(16)
                };

            cardContent.Add(
                transactionInfo);

            Grid.SetColumn(
                transactionInfo,
                0);

            cardContent.Add(
                buttons);

            Grid.SetColumn(
                buttons,
                1);

            var transactionCard =
                new Border
                {
                    BackgroundColor =
                        cardBackground,

                    StrokeThickness = 0,

                    StrokeShape =
                        new RoundRectangle
                        {
                            CornerRadius = 18
                        },

                    Content =
                        cardContent
                };

            TransactionsList.Children.Add(
                transactionCard);
        }

        if (balance > 0)
        {
            BalanceTitleLabel.Text =
                "They owe you";

            BalanceLabel.Text =
                $"+ {currency} {balance:0.00}";

            BalanceLabel.TextColor =
                Color.FromArgb("#34C759");
        }
        else if (balance < 0)
        {
            BalanceTitleLabel.Text =
                "You owe them";

            BalanceLabel.Text =
                $"- {currency} {Math.Abs(balance):0.00}";

            BalanceLabel.TextColor =
                Color.FromArgb("#FF453A");
        }
        else
        {
            BalanceTitleLabel.Text =
                "Settled";

            BalanceLabel.Text =
                $"{currency} 0.00";

            BalanceLabel.TextColor =
                secondaryText;
        }
    }

    private async void OnAddTransactionClicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new AddTransactionPage(
                _person));
    }
}