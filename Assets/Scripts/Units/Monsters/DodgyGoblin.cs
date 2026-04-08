using UnityEngine;

public class DodgyGoblin : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { 
            (2, 0),
        };
        actionList.AddRange(actions);
    }
}
