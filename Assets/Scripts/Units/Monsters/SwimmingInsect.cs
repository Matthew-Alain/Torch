using UnityEngine;

public class SwimmingInsect : BaseMonster
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
