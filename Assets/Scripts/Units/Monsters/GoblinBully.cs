using UnityEngine;

public class GoblinBully : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (2, 0),
            (3, 0),
            (4, 2)
        };
        actionList.AddRange(actions);
    }
}
