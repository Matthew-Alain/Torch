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

// public class MenuOption
// {
//     public string Label;
//     private System.Action action;
//     private System.Func<bool> isVisible;
//     private System.Func<bool> isEnabled;

//     public MenuOption(
//         string label,
//         System.Action action,
//         System.Func<bool> isVisible = null,
//         System.Func<bool> isEnabled = null)
//     {
//         Label = label;
//         this.action = action;
//         this.isVisible = isVisible ?? (() => true);
//         this.isEnabled = isEnabled ?? (() => true);
//     }

//     public bool IsVisible()
//     {
//         return isVisible();
//     }

//     public bool IsEnabled()
//     {
//         return CombatStateManager.Instance.GameState == GameState.PlayerTurn || CombatStateManager.Instance.GameState == GameState.SelectReaction && isEnabled();
//     }

//     public void TryExecute()
//     {
//         if (!(CombatStateManager.Instance.GameState == GameState.PlayerTurn || CombatStateManager.Instance.GameState == GameState.SelectReaction))
//         {
//             // Debug.Log("Input blocked by game state");
//             return;
//         }

//         if (!isEnabled())
//             return;

//         action?.Invoke();
//     }
// }