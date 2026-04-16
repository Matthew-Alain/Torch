using UnityEngine;

public class GoblinHexer : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (11, 2),
            (2, 0),
            (3, 0),
        };
        actionList.AddRange(actions);
    }
}
