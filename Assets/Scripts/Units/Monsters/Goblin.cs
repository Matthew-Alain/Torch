using UnityEngine;

public class Goblin : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { 
            (2, 0),
            (3, 0),
            (5, 2),
        };
        actionList.AddRange(actions);
    }
}
