namespace BloodBankPro.ViewModels;

public static class AppEvents
{
    public static event Action? DonorsChanged;

    public static void RaiseDonorsChanged() => DonorsChanged?.Invoke();
}
