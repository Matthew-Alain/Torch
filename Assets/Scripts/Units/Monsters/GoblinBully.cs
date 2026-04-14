using UnityEngine;

public class GoblinBully : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = {
            (0, 9),
            (3, 0)
        };
        actionList.AddRange(actions);
    }
}
