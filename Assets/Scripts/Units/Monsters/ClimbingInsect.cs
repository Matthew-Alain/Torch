using UnityEngine;

public class ClimbingInsect : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { 
            (2, 0), 
            (10, 2),
        };
        actionList.AddRange(actions);
    }
}
