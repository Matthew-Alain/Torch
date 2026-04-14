using UnityEngine;

public class GoblinArcher : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (9, 1),
            (3, 0)
        };
        actionList.AddRange(actions);
    }
}
