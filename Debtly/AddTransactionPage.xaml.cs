using Debtly.Models;
using Debtly.Services;

namespace Debtly;

public partial class AddTransactionPage : ContentPage
{
    private readonly Person _person;

    private bool _isFromPerson = false;

    public AddTransactionPage(Person person)
    {
        InitializeComponent();

        _person = person;

        CurrencyLabel.Text =
            CurrencyService.GetCurrencySymbol();

        UpdateButtons();

        Shell.SetTabBarIsVisible(this, false);
    }

    private void OnYouGaveClicked(
        object sender,
        EventArgs e)
    {
        _isFromPerson = false;

        UpdateButtons();
    }

    private void OnTheyGaveClicked(
        object sender,
        EventArgs e)
    {
        _isFromPerson = true;

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool isDark =
            Application.Current?.RequestedTheme == AppTheme.Dark;

        Color inactiveBackground =
            isDark
                ? Color.FromArgb("#202B36")
                : Colors.White;

        Color inactiveText =
            isDark
                ? Colors.White
                : Color.FromArgb("#1C1C1E");

        if (_isFromPerson)
        {
            TheyGaveButton.BackgroundColor =
                Color.FromArgb("#007AFF");

            TheyGaveButton.TextColor =
                Colors.White;

            YouGaveButton.BackgroundColor =
                inactiveBackground;

            YouGaveButton.TextColor =
                inactiveText;
        }
        else
        {
            YouGaveButton.BackgroundColor =
                Color.FromArgb("#007AFF");

            YouGaveButton.TextColor =
                Colors.White;

            TheyGaveButton.BackgroundColor =
                inactiveBackground;

            TheyGaveButton.TextColor =
                inactiveText;
        }
    }

    private async void OnSaveTransactionClicked(
        object sender,
        EventArgs e)
    {
        string amountText =
            AmountEntry.Text?.Trim() ?? "";

        if (!decimal.TryParse(
                amountText,
                out decimal amount) ||
            amount <= 0)
        {
            await DisplayAlert(
                "Invalid amount",
                "Please enter a valid amount.",
                "OK");

            return;
        }

        var transaction = new Transaction
        {
            PersonId = _person.Id,
            Amount = amount,
            IsFromPerson = _isFromPerson,
            Note = NoteEntry.Text?.Trim() ?? "",
            Date = DateTime.Now
        };

        await App.Database.AddTransactionAsync(
            transaction);

        await Navigation.PopAsync();
    }
}