using UnityEngine;

public class GoblinStriker : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { 
            (8, 2),
        };
        actionList.AddRange(actions);
    }
}
