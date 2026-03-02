public class MenuOption
{
    public string Label;
    public System.Action Action;
    public System.Func<bool> Condition; // optional visibility check

    public MenuOption(string label, System.Action action, System.Func<bool> condition = null)
    {
        Label = label;
        Action = action;
        Condition = condition;
    }

    public bool IsAvailable()
    {
        return Condition == null || Condition();
    }
}
