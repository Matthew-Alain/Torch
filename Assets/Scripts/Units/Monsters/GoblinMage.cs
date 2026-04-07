using UnityEngine;

public class GoblinMage : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (2, 0),
            (3, 0),
            (8, 2)
        };
        actionList.AddRange(actions);
    }
}
