using Debtly.Models;
using Debtly.Services;

namespace Debtly;

public partial class EditTransactionPage : ContentPage
{
    private readonly Transaction _transaction;

    private bool _isFromPerson;

    public EditTransactionPage(Transaction transaction)
    {
        InitializeComponent();

        _transaction = transaction;

        _isFromPerson = transaction.IsFromPerson;

        CurrencyLabel.Text =
            CurrencyService.GetCurrencySymbol();

        AmountEntry.Text =
            transaction.Amount.ToString("0.00");

        NoteEntry.Text =
            transaction.Note;

        UpdateButtons();

        Shell.SetTabBarIsVisible(this, false);
    }

    private void OnYouGaveClicked(object sender, EventArgs e)
    {
        _isFromPerson = false;

        UpdateButtons();
    }

    private void OnTheyGaveClicked(object sender, EventArgs e)
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
            // They gave - selected

            TheyGaveButton.BackgroundColor =
                Color.FromArgb("#007AFF");

            TheyGaveButton.TextColor =
                Colors.White;

            // You gave - not selected

            YouGaveButton.BackgroundColor =
                inactiveBackground;

            YouGaveButton.TextColor =
                inactiveText;
        }
        else
        {
            // You gave - selected

            YouGaveButton.BackgroundColor =
                Color.FromArgb("#007AFF");

            YouGaveButton.TextColor =
                Colors.White;

            // They gave - not selected

            TheyGaveButton.BackgroundColor =
                inactiveBackground;

            TheyGaveButton.TextColor =
                inactiveText;
        }
    }

    private async void OnSaveClicked(
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

        _transaction.Amount = amount;

        _transaction.IsFromPerson =
            _isFromPerson;

        _transaction.Note =
            NoteEntry.Text?.Trim() ?? "";

        await App.Database.UpdateTransactionAsync(
            _transaction);

        await Navigation.PopAsync();
    }
}