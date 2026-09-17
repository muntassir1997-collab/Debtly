using Debtly.Models;

namespace Debtly;

public partial class EditPersonPage : ContentPage
{
    private readonly Person _person;

    public EditPersonPage(Person person)
    {
        InitializeComponent();

        _person = person;

        NameEntry.Text = person.Name;

        Shell.SetTabBarIsVisible(this, false);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert(
                "Missing name",
                "Please enter a name.",
                "OK");

            return;
        }

        _person.Name = name;

        await App.Database.UpdatePersonAsync(_person);

        await Navigation.PopAsync();
    }
}