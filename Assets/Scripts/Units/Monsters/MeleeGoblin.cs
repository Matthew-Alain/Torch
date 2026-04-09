using UnityEngine;

public class MeleeGoblin : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { 
            (8, 0),
        };
        actionList.AddRange(actions);
    }
}
