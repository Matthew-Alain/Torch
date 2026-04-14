using UnityEngine;

public class GoblinDodger : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (0, 0),
            (2, 9),
        };
        actionList.AddRange(actions);
    }
}
