using Debtly.Models;
using Debtly.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Debtly;

public partial class PeoplePage : ContentPage
{
    private readonly List<Person> people = new();

    public PeoplePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadPeopleAsync();
    }

    private async Task LoadPeopleAsync()
    {
        people.Clear();

        var savedPeople =
            await App.Database.GetPeopleAsync();

        people.AddRange(savedPeople);

        await RefreshPeopleList(people);
    }

    private async Task<decimal> GetPersonBalanceAsync(
        int personId)
    {
        var transactions =
            await App.Database.GetTransactionsForPersonAsync(
                personId);

        decimal balance = 0;

        foreach (var transaction in transactions)
        {
            if (transaction.IsFromPerson)
            {
                balance -= transaction.Amount;
            }
            else
            {
                balance += transaction.Amount;
            }
        }

        return balance;
    }

    private async Task RefreshPeopleList(
        IEnumerable<Person> peopleToShow)
    {
        PeopleList.Children.Clear();

        var list =
            peopleToShow.ToList();

        PeopleCountLabel.Text =
            list.Count.ToString();

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

        Color editBackground =
            isDark
                ? Color.FromArgb("#1E3A5F")
                : Color.FromArgb("#EAF3FF");

        Color deleteBackground =
            isDark
                ? Color.FromArgb("#4A2928")
                : Color.FromArgb("#FFF0EF");

        if (list.Count == 0)
        {
            PeopleList.Children.Add(
                new Label
                {
                    Text = people.Count == 0
                        ? "No people yet"
                        : "No people found",

                    FontSize = 16,

                    TextColor = secondaryText,

                    HorizontalOptions =
                        LayoutOptions.Center
                });

            return;
        }

        string currency =
            CurrencyService.GetCurrencySymbol();

        foreach (var person in list)
        {
            decimal balance =
                await GetPersonBalanceAsync(person.Id);

            string balanceText;
            Color balanceColor;

            if (balance > 0)
            {
                balanceText =
                    $"They owe you  {currency} {balance:0.00}";

                balanceColor =
                    Color.FromArgb("#34C759");
            }
            else if (balance < 0)
            {
                balanceText =
                    $"You owe them  {currency} {Math.Abs(balance):0.00}";

                balanceColor =
                    Color.FromArgb("#FF453A");
            }
            else
            {
                balanceText = "Settled";

                balanceColor =
                    secondaryText;
            }

            var personInfo =
                new VerticalStackLayout
                {
                    Spacing = 4,

                    VerticalOptions =
                        LayoutOptions.Center,

                    Children =
                    {
                        new Label
                        {
                            Text = person.Name,

                            FontSize = 18,

                            FontAttributes =
                                FontAttributes.Bold,

                            TextColor =
                                primaryText,

                            LineBreakMode =
                                LineBreakMode.TailTruncation
                        },

                        new Label
                        {
                            Text = balanceText,

                            FontSize = 14,

                            TextColor =
                                balanceColor
                        }
                    }
                };

            var editButton =
                new Button
                {
                    Text = "Edit",

                    FontSize = 13,

                    FontAttributes =
                        FontAttributes.None,

                    HeightRequest = 34,

                    MinimumWidthRequest = 58,

                    Padding = new Thickness(10, 0),

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
                    new EditPersonPage(person));
            };

            var deleteButton =
                new Button
                {
                    Text = "Delete",

                    FontSize = 13,

                    FontAttributes =
                        FontAttributes.None,

                    HeightRequest = 34,

                    MinimumWidthRequest = 62,

                    Padding = new Thickness(10, 0),

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
                        "Delete Person",
                        $"Are you sure you want to delete {person.Name}?",
                        "Delete",
                        "Cancel");

                if (!confirm)
                    return;

                await App.Database.DeletePersonAsync(
                    person.Id);

                await LoadPeopleAsync();
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
                personInfo);

            Grid.SetColumn(
                personInfo,
                0);

            cardContent.Add(
                buttons);

            Grid.SetColumn(
                buttons,
                1);

            var personCard =
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

            var tapGesture =
                new TapGestureRecognizer();

            tapGesture.Tapped += async (sender, e) =>
            {
                await Navigation.PushAsync(
                    new PersonPage(person));
            };

            personCard.GestureRecognizers.Add(
                tapGesture);

            PeopleList.Children.Add(
                personCard);
        }
    }

    private async void OnSearchTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        string searchText =
            e.NewTextValue?.Trim() ?? "";

        var filteredPeople =
            people.Where(
                p => p.Name.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase));

        await RefreshPeopleList(
            filteredPeople);
    }

    private async void OnAddPersonClicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new AddPersonPage(
                OnPersonAdded));
    }

    private async void OnPersonAdded(
        Person person)
    {
        await App.Database.AddPersonAsync(
            person);

        await LoadPeopleAsync();
    }
}