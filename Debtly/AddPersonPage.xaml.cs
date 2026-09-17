using Debtly.Models;

namespace Debtly;

public partial class AddPersonPage : ContentPage
{
    private readonly Action<Person> _onPersonAdded;

    public AddPersonPage(Action<Person> onPersonAdded)
    {
        InitializeComponent();

        _onPersonAdded = onPersonAdded;

        Shell.SetTabBarIsVisible(this, false);
    }

    private async void OnAddPersonClicked(
        object sender,
        EventArgs e)
    {
        string name =
            NameEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert(
                "Missing name",
                "Please enter a name.",
                "OK");

            return;
        }

        var person = new Person
        {
            Name = name
        };

        _onPersonAdded(person);

        await Navigation.PopAsync();
    }
}