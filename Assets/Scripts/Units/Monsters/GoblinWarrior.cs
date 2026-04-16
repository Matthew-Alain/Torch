using UnityEngine;

public class GoblinWarrior : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (4, 2),
            (2, 0),
            (3, 0),
        };
        actionList.AddRange(actions);
    }
}
