using UnityEngine;

public class GoblinMinion : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { 
            (5, 2),
            (2, 0),
            (3, 0),
        };
        actionList.AddRange(actions);
    }
}
