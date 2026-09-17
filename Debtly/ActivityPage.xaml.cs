using Debtly.Models;
using Debtly.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Debtly;

public partial class ActivityPage : ContentPage
{
    public ActivityPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadActivityAsync();
    }

    private async Task LoadActivityAsync()
    {
        var transactions =
            await App.Database.GetAllTransactionsAsync();

        var people =
            await App.Database.GetPeopleAsync();

        ActivityList.Children.Clear();

        string currency =
            CurrencyService.GetCurrencySymbol();

        bool isDark =
            Application.Current?.RequestedTheme == AppTheme.Dark;

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

        if (transactions.Count == 0)
        {
            ActivityList.Children.Add(
                new Label
                {
                    Text = "No transactions yet",
                    FontSize = 16,
                    TextColor = secondaryText,
                    HorizontalOptions =
                        LayoutOptions.Center
                });

            return;
        }

        foreach (var transaction in transactions)
        {
            var person = people.FirstOrDefault(
                p => p.Id == transaction.PersonId);

            string personName =
                person?.Name ?? "Unknown";

            string amountText =
                transaction.IsFromPerson
                    ? $"- {currency} {transaction.Amount:0.00}"
                    : $"+ {currency} {transaction.Amount:0.00}";

            string description =
                transaction.IsFromPerson
                    ? "They gave"
                    : "You gave";

            string details =
                string.IsNullOrWhiteSpace(transaction.Note)
                    ? transaction.Date.ToString("dd MMM yyyy")
                    : transaction.Note;

            var transactionInfo =
                new VerticalStackLayout
                {
                    Spacing = 5,

                    Children =
                    {
                        new Label
                        {
                            Text = personName,
                            FontSize = 18,
                            FontAttributes =
                                FontAttributes.Bold,
                            TextColor = primaryText
                        },

                        new Label
                        {
                            Text = amountText,
                            FontSize = 20,
                            FontAttributes =
                                FontAttributes.Bold,
                            TextColor = primaryText
                        },

                        new Label
                        {
                            Text = description,
                            FontSize = 14,
                            TextColor = secondaryText
                        },

                        new Label
                        {
                            Text = details,
                            FontSize = 14,
                            TextColor = secondaryText
                        }
                    }
                };

            var editButton =
                new Button
                {
                    Text = "Edit",
                    FontSize = 14,
                    HeightRequest = 40,
                    CornerRadius = 12,
                    BackgroundColor =
                        Color.FromArgb("#007AFF"),
                    TextColor = Colors.White
                };

            editButton.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(
                    new EditTransactionPage(transaction));
            };

            var deleteButton =
                new Button
                {
                    Text = "Delete",
                    FontSize = 14,
                    HeightRequest = 40,
                    CornerRadius = 12,
                    BackgroundColor =
                        Color.FromArgb("#FF3B30"),
                    TextColor = Colors.White
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

                await LoadActivityAsync();
            };

            var buttons =
                new HorizontalStackLayout
                {
                    Spacing = 8,
                    HorizontalOptions =
                        LayoutOptions.End,

                    Children =
                    {
                        editButton,
                        deleteButton
                    }
                };

            var cardContent =
                new VerticalStackLayout
                {
                    Spacing = 12,
                    Padding = 18,

                    Children =
                    {
                        transactionInfo,
                        buttons
                    }
                };

            var transactionCard =
                new Border
                {
                    BackgroundColor = cardBackground,
                    StrokeThickness = 0,

                    StrokeShape =
                        new RoundRectangle
                        {
                            CornerRadius = 20
                        },

                    Content = cardContent
                };

            ActivityList.Children.Add(
                transactionCard);
        }
    }
}