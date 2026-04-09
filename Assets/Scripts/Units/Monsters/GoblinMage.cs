using UnityEngine;

public class GoblinMage : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (2, 0),
            (3, 0),
            (7, 2)
        };
        actionList.AddRange(actions);
    }
}
