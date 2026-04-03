// public class MenuOption
// {
//     public string Label;
//     public System.Action Action;
//     public System.Func<bool> Condition; // optional visibility check

//     public MenuOption(string label, System.Action action, System.Func<bool> condition = null)
//     {
//         Label = label;
//         Action = action;
//         Condition = condition;
//     }

//     public bool IsAvailable()
//     {
//         return Condition == null || Condition();
//     }
// }

public class MenuOption
{
    public string Label;
    public System.Action Action;
    public System.Func<bool> IsVisible;
    public System.Func<bool> IsEnabled;

    public MenuOption(
        string label,
        System.Action action,
        System.Func<bool> isVisible = null,
        System.Func<bool> isEnabled = null)
    {
        Label = label;
        Action = action;
        IsVisible = isVisible ?? (() => true);
        IsEnabled = isEnabled ?? (() => true);
    }
}