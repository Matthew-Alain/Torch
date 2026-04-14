using UnityEngine;

public class GoblinMage : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (7, 2),
            (2, 0),
            (3, 0),
        };
        actionList.AddRange(actions);
    }
}
