using System.Text;
using Debtly.Models;

namespace Debtly;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        string savedCurrency =
            Preferences.Default.Get(
                "Currency",
                "₪ ILS");

        CurrencyPicker.SelectedItem =
            savedCurrency;

        string savedAppearance =
            Preferences.Default.Get(
                "Appearance",
                "System");

        AppearancePicker.SelectedItem =
            savedAppearance;

        ApplyAppearance(savedAppearance);
    }

    private void CurrencyPicker_SelectedIndexChanged(
        object sender,
        EventArgs e)
    {
        if (CurrencyPicker.SelectedItem is string currency)
        {
            Preferences.Default.Set(
                "Currency",
                currency);
        }
    }

    private void AppearancePicker_SelectedIndexChanged(
        object sender,
        EventArgs e)
    {
        if (AppearancePicker.SelectedItem is not string appearance)
            return;

        Preferences.Default.Set(
            "Appearance",
            appearance);

        ApplyAppearance(appearance);
    }

    private void ApplyAppearance(
        string appearance)
    {
        if (Application.Current == null)
            return;

        Application.Current.UserAppTheme =
            appearance switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
    }

    private async void OnExportDataClicked(
        object sender,
        EventArgs e)
    {
        try
        {
            var people =
                await App.Database.GetPeopleAsync();

            var transactions =
                await App.Database.GetAllTransactionsAsync();

            var peopleDictionary =
                people.ToDictionary(
                    p => p.Id,
                    p => p.Name);

            var csv =
                new StringBuilder();

            csv.AppendLine(
                "Person,Amount,Direction,Note,Date");

            foreach (var transaction in transactions)
            {
                string personName =
                    peopleDictionary.TryGetValue(
                        transaction.PersonId,
                        out string? name)
                        ? name
                        : "Unknown";

                string direction =
                    transaction.IsFromPerson
                        ? "They gave"
                        : "You gave";

                string note =
                    transaction.Note
                        .Replace("\"", "\"\"");

                csv.AppendLine(
                    $"\"{personName}\"," +
                    $"{transaction.Amount:0.00}," +
                    $"\"{direction}\"," +
                    $"\"{note}\"," +
                    $"\"{transaction.Date:yyyy-MM-dd HH:mm}\"");
            }

            string fileName =
                $"Debtly_Export_{DateTime.Now:yyyy-MM-dd}.csv";

            string filePath =
                Path.Combine(
                    FileSystem.CacheDirectory,
                    fileName);

            await File.WriteAllTextAsync(
                filePath,
                csv.ToString(),
                Encoding.UTF8);

            await Share.Default.RequestAsync(
                new ShareFileRequest
                {
                    Title = "Export Debtly Data",
                    File = new ShareFile(filePath)
                });
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Export failed",
                ex.Message,
                "OK");
        }
    }

    private async void OnDeleteAllDataClicked(
        object sender,
        EventArgs e)
    {
        bool firstConfirmation =
            await DisplayAlert(
                "Delete All Data",
                "This will permanently delete all people and transactions.",
                "Continue",
                "Cancel");

        if (!firstConfirmation)
            return;

        bool secondConfirmation =
            await DisplayAlert(
                "Are you sure?",
                "All your Debtly data will be deleted. This cannot be undone.",
                "Delete Everything",
                "Cancel");

        if (!secondConfirmation)
            return;

        try
        {
            await App.Database.DeleteAllDataAsync();

            await DisplayAlert(
                "Data Deleted",
                "All people and transactions have been deleted.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Delete failed",
                ex.Message,
                "OK");
        }
    }
}