using Debtly.Data;

namespace Debtly;

public partial class App : Application
{
    public static DatabaseService Database { get; } =
        new DatabaseService();

    public App()
    {
        InitializeComponent();

        ApplySavedAppearance();

        Task.Run(async () =>
        {
            await Database.InitializeAsync();
        });
    }

    private void ApplySavedAppearance()
    {
        string appearance =
            Preferences.Default.Get(
                "Appearance",
                "System");

        UserAppTheme =
            appearance switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}